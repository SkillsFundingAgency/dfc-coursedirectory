﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Security;
using FluentValidation;
using MediatR;
using OneOf;
using OneOf.Types;
using SqlModels = Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using SqlQueries = Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Results;

namespace Dfc.CourseDirectory.WebV2.Features.Providers.EditProviderType
{
    public class Query : IRequest<ViewModel>
    {
        public Guid ProviderId { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, ModelWithErrors<ConfirmViewModel>, ConfirmViewModel, Cancel, Success>>
    {
        public Guid ProviderId { get; set; }
        public ProviderType ProviderType { get; set; }
        public IReadOnlyList<Guid> SelectedProviderTLevelDefinitionIds { get; set; }
        public IReadOnlyList<Guid> SelectedNonLarsSubTypeIds { get; set; }
        public string AffectedTLevelIdsChecksum { get; set; }
        public bool? Confirm { get; set; }
    }

    public class ViewModel : Command
    {
        public IEnumerable<ProviderTLevelDefinitionViewModel> ProviderTLevelDefinitions { get; set; }
        public IEnumerable<ProviderNonLarsSubTypeViewModel> ProviderNonLarsSubTypes { get; set; }
    }

    public class ConfirmViewModel : Command
    {
        public IEnumerable<AffectedItemCountViewModel> AffectedItemCounts { get; set; }
    }

    public class AffectedItemCountViewModel
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }

    public class Cancel
    { }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, ModelWithErrors<ConfirmViewModel>, ConfirmViewModel, Cancel, Success>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IProviderInfoCache _providerInfoCache;
        private readonly IClock _clock;
        private readonly ICurrentUserProvider _currentUserProvider;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderInfoCache providerInfoCache,
            IClock clock,
            ICurrentUserProvider currentUserProvider)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerInfoCache = providerInfoCache;
            _clock = clock;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var provider = await _sqlQueryDispatcher.ExecuteQuery(new GetProviderById()
            {
                ProviderId = request.ProviderId
            });

            var tLevelDefinitions = await _sqlQueryDispatcher.ExecuteQuery(new GetTLevelDefinitions());
            var providerTLevelDefinitions = provider.ProviderType.HasFlag(ProviderType.TLevels)
                ? await _sqlQueryDispatcher.ExecuteQuery(new SqlQueries.GetTLevelDefinitionsForProvider { ProviderId = request.ProviderId })
                : Enumerable.Empty<SqlModels.TLevelDefinition>();

            var nonLarsSubTypes = await _sqlQueryDispatcher.ExecuteQuery(new GetAllNonLarsSubTypes());
            var providerSubTypes = provider.ProviderType.HasFlag(ProviderType.NonLARS)
                ? await _sqlQueryDispatcher.ExecuteQuery(new SqlQueries.GetNonLarsSubTypeForProvider { ProviderId = request.ProviderId })
                : Enumerable.Empty<SqlModels.NonLarsSubType>();

            return CreateViewModel(provider.ProviderId, provider.ProviderType, nonLarsSubTypes, providerSubTypes.Select(pd => pd.NonLarsSubTypeId), tLevelDefinitions, providerTLevelDefinitions.Select(pd => pd.TLevelDefinitionId));
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, ModelWithErrors<ConfirmViewModel>, ConfirmViewModel, Cancel, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.Confirm == false)
            {
                return new Cancel();
            }

            var tLevelDefinitions = await _sqlQueryDispatcher.ExecuteQuery(new GetTLevelDefinitions());

            var nonLarsSubtypes = await _sqlQueryDispatcher.ExecuteQuery(new GetAllNonLarsSubTypes());

            var validateTLevels = await new CommandValidator(tLevelDefinitions).ValidateAsync(request);

            var validateSubtypes = await new CommandValidator(nonLarsSubtypes).ValidateAsync(request);

            IList<ValidationFailure> failureList = validateTLevels.Errors;
            foreach (var error in validateSubtypes.Errors)
            {
                failureList.Add(error);
            }

            FluentValidation.Results.ValidationResult validationResult = new FluentValidation.Results.ValidationResult(failureList);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<ViewModel>(
                    CreateViewModel(request.ProviderId, request.ProviderType,nonLarsSubtypes, request.SelectedNonLarsSubTypeIds, tLevelDefinitions, request.SelectedProviderTLevelDefinitionIds),
                    validationResult);
            }

            var tLevels = await _sqlQueryDispatcher.ExecuteQuery(new GetTLevelsForProvider { ProviderId = request.ProviderId });

            var affectedTLevels = tLevels
                .Where(t => !request.ProviderType.HasFlag(ProviderType.TLevels)
                    || !request.SelectedProviderTLevelDefinitionIds.Contains(t.TLevelDefinition.TLevelDefinitionId))
                .ToArray();

            if (affectedTLevels.Any())
            {
                var affectedTLevelsChecksum = GenerateAffectedTLevelIdsChecksum(affectedTLevels);
                var confirmValidationResult = await new ConfirmCommandValidator(affectedTLevelsChecksum).ValidateAsync(request);

                if (!confirmValidationResult.IsValid)
                {
                    var confirmViewModel = new ConfirmViewModel
                    {
                        ProviderId = request.ProviderId,
                        ProviderType = request.ProviderType,
                        SelectedNonLarsSubTypeIds = request.SelectedNonLarsSubTypeIds,
                        SelectedProviderTLevelDefinitionIds = request.SelectedProviderTLevelDefinitionIds,
                        AffectedTLevelIdsChecksum = affectedTLevelsChecksum,
                        Confirm = null,
                        AffectedItemCounts = affectedTLevels
                            .GroupBy(t => t.TLevelDefinition.TLevelDefinitionId)
                            .Select(g => new AffectedItemCountViewModel { Name = g.First().TLevelDefinition.Name, Count = g.Count() })
                    };

                    // Using AffectedTLevelIdsChecksum to establish if this the first call to confirm
                    if (request.AffectedTLevelIdsChecksum == null)
                    {
                        return confirmViewModel;
                    }

                    return new ModelWithErrors<ConfirmViewModel>(
                        confirmViewModel,
                        confirmValidationResult);
                }
            }

            await _sqlQueryDispatcher.ExecuteQuery(new UpdateProviderType()
            {
                ProviderId = request.ProviderId,
                ProviderType = request.ProviderType,
                UpdatedBy = _currentUserProvider.GetCurrentUser(),
                UpdatedOn = _clock.UtcNow.ToLocalTime()
            });

            var (_, RemovedTLevelDefinitionIds) = await _sqlQueryDispatcher.ExecuteQuery(
                new SetProviderTLevelDefinitions
                {
                    ProviderId = request.ProviderId,
                    TLevelDefinitionIds = request.ProviderType.HasFlag(ProviderType.TLevels)
                        ? request.SelectedProviderTLevelDefinitionIds ?? Enumerable.Empty<Guid>()
                        : Enumerable.Empty<Guid>()
                });

            if (RemovedTLevelDefinitionIds.Count != 0)
            {
                await _sqlQueryDispatcher.ExecuteQuery(new DeleteTLevelsForProviderWithTLevelDefinitions()
                {
                    ProviderId = request.ProviderId,
                    TLevelDefinitionIds = RemovedTLevelDefinitionIds,
                    DeletedBy = _currentUserProvider.GetCurrentUser(),
                    DeletedOn = _clock.UtcNow
                });
            }

            //var (_, RemovedNonLarsSubTypeIds) = await _sqlQueryDispatcher.ExecuteQuery(
            //   new SetProviderNonLarsSubTypes
            //   {
            //       ProviderId = request.ProviderId,
            //       NonLarsSubTypeIds = request.ProviderType.HasFlag(ProviderType.NonLARS)
            //           ? request.SelectedNonLarsSubTypeIds ?? Enumerable.Empty<Guid>()
            //           : Enumerable.Empty<Guid>()
            //   });

            //if (RemovedNonLarsSubTypeIds.Count != 0)
            //{
            //    await _sqlQueryDispatcher.ExecuteQuery(new DeleteTLevelsForProviderWithTLevelDefinitions()
            //    {
            //        ProviderId = request.ProviderId,
            //        TLevelDefinitionIds = RemovedTLevelDefinitionIds,
            //        DeletedBy = _currentUserProvider.GetCurrentUser(),
            //        DeletedOn = _clock.UtcNow
            //    });
            //}

            // Remove this provider from the cache - subsequent requests will re-fetch updated record
            await _providerInfoCache.Remove(request.ProviderId);

            return new Success();
        }

        private static ViewModel CreateViewModel(
            Guid providerId,
            ProviderType providerType,
            IEnumerable<SqlModels.NonLarsSubType> nonLarsSubType,
            IEnumerable<Guid> providerSubTypes,
            IEnumerable<SqlModels.TLevelDefinition> tLevelDefinitions,
            IEnumerable<Guid> providerTLevelDefinitions) => new ViewModel
            {
                ProviderId = providerId,
                ProviderType = providerType,
                ProviderNonLarsSubTypes = nonLarsSubType.Select(n => new ProviderNonLarsSubTypeViewModel { 
                    NonLarsSubTypeId = n.NonLarsSubTypeId,
                    Name = n.Name,
                    Selected = providerSubTypes?.Any(id => id == n.NonLarsSubTypeId) ?? false
                }),
                ProviderTLevelDefinitions = tLevelDefinitions.Select(d => new ProviderTLevelDefinitionViewModel
                {
                    TLevelDefinitionId = d.TLevelDefinitionId,
                    Name = d.Name,
                    Selected = providerTLevelDefinitions?.Any(id => id == d.TLevelDefinitionId) ?? false
                })
            };

        private static string GenerateAffectedTLevelIdsChecksum(IEnumerable<SqlModels.TLevel> affectedTLevels) =>
            Convert.ToBase64String(affectedTLevels.OrderBy(t => t.TLevelId).SelectMany(t => t.TLevelId.ToByteArray()).ToArray());

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(IEnumerable<SqlModels.TLevelDefinition> tLevelDefinitions)
            {
                When(c => c.ProviderType.HasFlag(ProviderType.TLevels), () =>
                {
                    RuleFor(c => c.SelectedProviderTLevelDefinitionIds)
                        .NotEmpty()
                        .WithMessage("Select the T Levels this provider can offer");

                    When(c => c.SelectedProviderTLevelDefinitionIds?.Any() ?? false, () =>
                    {
                        RuleFor(c => c.SelectedProviderTLevelDefinitionIds)
                            .Must(ids => ids.All(id => tLevelDefinitions.Any(d => d.TLevelDefinitionId == id)))
                            .WithMessage("Select a valid T Level");
                    });

                });
            }
            public CommandValidator(IEnumerable<SqlModels.NonLarsSubType> nonLarsSubTypes)
            {
                When(c => c.ProviderType.HasFlag(ProviderType.NonLARS), () =>
                {
                    RuleFor(c => c.SelectedNonLarsSubTypeIds)
                        .NotEmpty()
                        .WithMessage("Select Non LARS Course this provider can offer");

                    When(c => c.SelectedNonLarsSubTypeIds?.Any() ?? false, () =>
                    {
                        RuleFor(c => c.SelectedNonLarsSubTypeIds)
                            .Must(ids => ids.All(id => nonLarsSubTypes.Any(d => d.NonLarsSubTypeId == id)))
                            .WithMessage("Select a valid Non LARS Subtype");
                    });

                });
            }
        }

        private class ConfirmCommandValidator : AbstractValidator<Command>
        {
            public ConfirmCommandValidator(string affectedTLevelsChecksum)
            {
                RuleFor(c => c.AffectedTLevelIdsChecksum)
                    .Equal(affectedTLevelsChecksum)
                    .OverridePropertyName(nameof(ConfirmViewModel.AffectedItemCounts))
                    .WithMessage("The affected T Levels have changed");

                RuleFor(c => c.Confirm)
                    .Equal(true)
                    .WithMessage("Select yes to permanently delete");
            }
        }
    }
}
