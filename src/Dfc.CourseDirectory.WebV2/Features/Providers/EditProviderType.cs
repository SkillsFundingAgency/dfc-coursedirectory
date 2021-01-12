using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
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
        public IReadOnlyList<Guid> ConfirmAffectedTLevelIds { get; set; }
        public bool? Confirm { get; set; }
    }

    public class ViewModel : Command
    {
        public IEnumerable<ProviderTLevelDefinitionViewModel> ProviderTLevelDefinitions { get; set; }
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
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IProviderInfoCache _providerInfoCache;
        private readonly IClock _clock;
        private readonly ICurrentUserProvider _currentUserProvider;

        public Handler(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderInfoCache providerInfoCache,
            IClock clock,
            ICurrentUserProvider currentUserProvider)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerInfoCache = providerInfoCache;
            _clock = clock;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderById()
            {
                ProviderId = request.ProviderId
            });

            var tLevelDefinitions = await _sqlQueryDispatcher.ExecuteQuery(new SqlQueries.GetTLevelDefinitions());
            var providerTLevelDefinitions = provider.ProviderType.HasFlag(ProviderType.TLevels)
                ? await _sqlQueryDispatcher.ExecuteQuery(new SqlQueries.GetTLevelDefinitionsForProvider { ProviderId = request.ProviderId })
                : Enumerable.Empty<SqlModels.TLevelDefinition>();

            return CreateViewModel(provider.Id, provider.ProviderType, tLevelDefinitions, providerTLevelDefinitions.Select(pd => pd.TLevelDefinitionId));
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, ModelWithErrors<ConfirmViewModel>, ConfirmViewModel, Cancel, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.Confirm == false)
            {
                return new Cancel();
            }

            var tLevelDefinitions = await _sqlQueryDispatcher.ExecuteQuery(new SqlQueries.GetTLevelDefinitions());

            var validationResult = await new CommandValidator(tLevelDefinitions).ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<ViewModel>(
                    CreateViewModel(request.ProviderId, request.ProviderType, tLevelDefinitions, request.SelectedProviderTLevelDefinitionIds),
                    validationResult);
            }

            var tLevels = await _sqlQueryDispatcher.ExecuteQuery(new SqlQueries.GetTLevelsForProvider { ProviderId = request.ProviderId });

            var affectedTLevels = tLevels
                .Where(t => !request.ProviderType.HasFlag(ProviderType.TLevels)
                    || !request.SelectedProviderTLevelDefinitionIds.Contains(t.TLevelDefinition.TLevelDefinitionId))
                .ToArray();

            var confirmValidationResult = await new ConfirmCommandValidator(affectedTLevels).ValidateAsync(request);

            if (!confirmValidationResult.IsValid)
            {
                var confirmViewModel = new ConfirmViewModel
                {
                    ProviderId = request.ProviderId,
                    ProviderType = request.ProviderType,
                    SelectedProviderTLevelDefinitionIds = request.SelectedProviderTLevelDefinitionIds,
                    ConfirmAffectedTLevelIds = affectedTLevels.Select(t => t.TLevelId).ToArray(),
                    Confirm = null,
                    AffectedItemCounts = affectedTLevels
                        .GroupBy(t => t.TLevelDefinition.TLevelDefinitionId)
                        .Select(g => new AffectedItemCountViewModel { Name = g.First().TLevelDefinition.Name, Count = g.Count() })
                };

                // Using ConfirmAffectedTLevelIds to establish if this the first call to confirm
                if (request.ConfirmAffectedTLevelIds == null || !request.ConfirmAffectedTLevelIds.Any())
                {
                    return confirmViewModel;
                }

                return new ModelWithErrors<ConfirmViewModel>(
                        confirmViewModel,
                        confirmValidationResult);
            }

            await _cosmosDbQueryDispatcher.ExecuteQuery(new UpdateProviderType()
            {
                ProviderId = request.ProviderId,
                ProviderType = request.ProviderType
            });

            var (_, RemovedTLevelDefinitionIds) = await _sqlQueryDispatcher.ExecuteQuery(
                new SqlQueries.SetProviderTLevelDefinitions
                {
                    ProviderId = request.ProviderId,
                    TLevelDefinitionIds = request.ProviderType.HasFlag(ProviderType.TLevels)
                        ? request.SelectedProviderTLevelDefinitionIds ?? Enumerable.Empty<Guid>()
                        : Enumerable.Empty<Guid>()
                });

            if (RemovedTLevelDefinitionIds.Count != 0)
            {
                await _sqlQueryDispatcher.ExecuteQuery(new SqlQueries.DeleteTLevelsForProviderWithTLevelDefinitions()
                {
                    ProviderId = request.ProviderId,
                    TLevelDefinitionIds = RemovedTLevelDefinitionIds,
                    DeletedBy = _currentUserProvider.GetCurrentUser(),
                    DeletedOn = _clock.UtcNow
                });
            }

            // Remove this provider from the cache - subsequent requests will re-fetch updated record
            await _providerInfoCache.Remove(request.ProviderId);

            return new Success();
        }

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
        }

        private class ConfirmCommandValidator : AbstractValidator<Command>
        {
            public ConfirmCommandValidator(IEnumerable<SqlModels.TLevel> affectedTLevels)
            {
                When(_ => affectedTLevels.Any(), () =>
                {
                    RuleFor(c => c.ConfirmAffectedTLevelIds)
                        .Must(ids => ids?.OrderBy(i => i).SequenceEqual(affectedTLevels.Select(t => t.TLevelId).OrderBy(i => i)) ?? false)
                        .OverridePropertyName(nameof(ConfirmViewModel.AffectedItemCounts))
                        .WithMessage("The affected T Levels have changed");

                    When(c => c.ConfirmAffectedTLevelIds?.Any() ?? false, () =>
                    {
                        RuleFor(c => c.Confirm)
                            .Equal(true)
                            .WithMessage("Select yes to permanently delete");
                    });
                });
            }
        }

        private static ViewModel CreateViewModel(
            Guid providerId,
            ProviderType providerType,
            IEnumerable<SqlModels.TLevelDefinition> tLevelDefinitions,
            IEnumerable<Guid> providerTLevelDefinitions) => new ViewModel
            {
                ProviderId = providerId,
                ProviderType = providerType,
                ProviderTLevelDefinitions = tLevelDefinitions.Select(d => new ProviderTLevelDefinitionViewModel
                {
                    TLevelDefinitionId = d.TLevelDefinitionId,
                    Name = d.Name,
                    Selected = providerTLevelDefinitions?.Any(id => id == d.TLevelDefinitionId) ?? false
                })
            };
    }
}
