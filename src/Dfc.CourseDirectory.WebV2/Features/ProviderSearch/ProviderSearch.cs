using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;
using OneOf;
using OneOf.Types;
using SearchModels = Dfc.CourseDirectory.Core.Search.Models;

namespace Dfc.CourseDirectory.WebV2.Features.ProviderSearch
{
    public class Query : IRequest<ViewModel>
    {
        public string SearchQuery { get; set; }
    }

    public class ViewModel : Query
    {
        public IReadOnlyCollection<ProviderSearchResultViewModel> ProviderSearchResults { get; set; }
    }

    public class ProviderSearchResultViewModel
    {
        public Guid ProviderId { get; set; }
        public string Name { get; set; }
        public string Postcode { get; set; }
        public string Town { get; set; }
        public string Ukprn { get; set; }
        public ProviderStatus? Status { get; set; }
        public string ProviderStatus { get; set; }
    }

    public class OnboardProviderCommand : IRequest<OneOf<NotFound, Success>>
    {
        public Guid ProviderId { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<OnboardProviderCommand, OneOf<NotFound, Success>>
    {
        private readonly ISearchClient<SearchModels.Provider> _providerSearchClient;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;

        public Handler(
            ISearchClient<SearchModels.Provider> providerSearchClient,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ICurrentUserProvider currentUserProvider,
            IClock clock)
        {
            _providerSearchClient = providerSearchClient;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            // When no search query is in the request return null Providers so we don't show the results table
            if (request.SearchQuery == null)
            {
                return new ViewModel
                {
                    ProviderSearchResults = null
                };
            }

            // Return empty results when they search query is empty or whitespace
            if (string.IsNullOrWhiteSpace(request.SearchQuery))
            {
                return new ViewModel
                {
                    ProviderSearchResults = Array.Empty<ProviderSearchResultViewModel>()
                };
            }

            var result = await _providerSearchClient.Search(new ProviderSearchQuery
            {
                SearchText = request.SearchQuery
            });

            return new ViewModel
            {
                ProviderSearchResults = result.Items.Select(r => r.Record).Select(p => new ProviderSearchResultViewModel
                {
                    ProviderId = p.Id,
                    Name = p.Name,
                    Postcode = p.Postcode,
                    Town = p.Town,
                    Ukprn = p.UKPRN,
                    Status = (ProviderStatus)p.Status,
                    ProviderStatus = p.ProviderStatus
                }).ToArray()
            };
        }

        public async Task<OneOf<NotFound, Success>> Handle(OnboardProviderCommand request, CancellationToken cancellationToken)
        {
            var result = await _cosmosDbQueryDispatcher.ExecuteQuery(new UpdateProviderOnboarded
            {
                ProviderId = request.ProviderId,
                UpdatedBy = _currentUserProvider.GetCurrentUser(),
                UpdatedDateTime = _clock.UtcNow.ToLocalTime()
            });

            return result.Match<OneOf<NotFound, Success>>(
                notFound => notFound,
                _ => new Success(),
                success => success);
        }
    }
}
