using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
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

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        public Guid ProviderId { get; set; }
        public ProviderType ProviderType { get; set; }
        public IEnumerable<Guid> SelectedProviderTLevelDefinitionIds { get; set; }
    }

    public class ViewModel : Command
    {
        public IEnumerable<ProviderTLevelDefinitionViewModel> ProviderTLevelDefinitions { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IProviderInfoCache _providerInfoCache;

        public Handler(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher, ISqlQueryDispatcher sqlQueryDispatcher, IProviderInfoCache providerInfoCache)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerInfoCache = providerInfoCache;
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

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tLevelDefinitions = await _sqlQueryDispatcher.ExecuteQuery(new SqlQueries.GetTLevelDefinitions());

            var validationResult = await new CommandValidator(tLevelDefinitions).ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<ViewModel>(
                    CreateViewModel(request.ProviderId, request.ProviderType, tLevelDefinitions, request.SelectedProviderTLevelDefinitionIds),
                    validationResult);
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
                    TLevelDefinitionIds = RemovedTLevelDefinitionIds
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
