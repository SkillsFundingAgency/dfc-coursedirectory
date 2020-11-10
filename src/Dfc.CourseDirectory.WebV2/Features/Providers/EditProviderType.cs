using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using MediatR;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.Providers.EditProviderType
{
    public class Query : IRequest<Command>
    {
        public Guid ProviderId { get; set; }
    }

    public class Command : IRequest<Success>
    {
        public Guid ProviderId { get; set; }
        public ProviderType ProviderType { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, Command>,
        IRequestHandler<Command, Success>
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IProviderInfoCache _providerInfoCache;

        public Handler(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher, IProviderInfoCache providerInfoCache)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _providerInfoCache = providerInfoCache;
        }

        public async Task<Command> Handle(Query request, CancellationToken cancellationToken)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderById()
            {
                ProviderId = request.ProviderId
            });

            return new Command()
            {
                ProviderId = provider.Id,
                ProviderType = provider.ProviderType
            };
        }

        public async Task<Success> Handle(Command request, CancellationToken cancellationToken)
        {
            await _cosmosDbQueryDispatcher.ExecuteQuery(new UpdateProviderType()
            {
                ProviderId = request.ProviderId,
                ProviderType = request.ProviderType
            });

            // Remove this provider from the cache - subsequent requests will re-fetch updated record
            await _providerInfoCache.Remove(request.ProviderId);

            return new Success();
        }
    }
}
