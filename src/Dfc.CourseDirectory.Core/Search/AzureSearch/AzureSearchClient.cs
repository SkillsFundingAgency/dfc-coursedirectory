using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Search.Documents;

namespace Dfc.CourseDirectory.Core.Search.AzureSearch
{
    public class AzureSearchClient<TResult> : ISearchClient<TResult>
    {
        private readonly SearchClient _client;

        public AzureSearchClient(SearchClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<SearchResult<TResult>> Search<TQuery>(TQuery query)
            where TQuery : ISearchQuery<TResult>
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (!(query is IAzureSearchQuery<TResult> azureQuery))
            {
                throw new ArgumentException($"{query.GetType().Name} does not implement {nameof(IAzureSearchQuery<TResult>)}.", nameof(query));
            }

            var generatedQuery = azureQuery.GenerateSearchQuery();

            var searchResults = await _client.SearchAsync<TResult>(generatedQuery.SearchText, generatedQuery.Options);

            return new SearchResult<TResult>
            {
                Results = searchResults.Value.GetResults().Select(r => r.Document).ToArray(),
                Facets = searchResults.Value.Facets?.ToDictionary(f => f.Key, f => (IReadOnlyDictionary<object, long?>)f.Value.ToDictionary(ff => ff.Value, ff => ff.Count)),
                TotalCount = searchResults.Value.TotalCount
            };
        }
    }
}