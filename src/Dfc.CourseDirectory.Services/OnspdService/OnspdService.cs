using System;
using System.Linq;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Models.Onspd;
using Dfc.CourseDirectory.Services.Interfaces.OnspdService;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Services.OnspdService
{
    public class OnspdService : IOnspdService
    {
        private readonly ILogger<OnspdService> _logger;
        private readonly string _searchServiceName;
        private readonly string _searchServiceQueryApiKey;
        private readonly string _indexName;

        public OnspdService(ILogger<OnspdService> logger, IOptions<OnspdSearchSettings> settings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(settings, nameof(settings));

            _logger = logger;

            _searchServiceName = settings.Value.SearchServiceName;
            _searchServiceQueryApiKey = settings.Value.SearchServiceQueryApiKey;
            _indexName = settings.Value.IndexName;
        }

        public IResult<IOnspdSearchResult> GetOnspdData(IOnspdSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Onspd search criteria", criteria);
                _logger.LogInformationObject("Onspd search service name", _searchServiceName);

                ISearchIndexClient indexClientForQueries = CreateSearchIndexClient();
                var onspdData = RunQuery(indexClientForQueries, criteria.Postcode);

                var searchResult = new OnspdSearchResult(onspdData)
                {
                    Value = onspdData
                };

                return Result.Ok<IOnspdSearchResult>(searchResult);

            }
            catch (Exception e)
            {
                _logger.LogException("Onspd search service unknown error", e);
                return Result.Fail<IOnspdSearchResult>("Onspd search service unknown error");
            }
        }

        private SearchIndexClient CreateSearchIndexClient()
        {
            var indexClient = new SearchIndexClient(_searchServiceName , _indexName , new SearchCredentials(_searchServiceQueryApiKey));
            return indexClient;
        }

        public Onspd RunQuery(ISearchIndexClient indexClient, string postcode)
        {
            SearchParameters parameters;
            DocumentSearchResult<Onspd> results;

            parameters = new SearchParameters
            {
                Select = new[] { "pcd", "pcd2", "pcds", "Parish", "LocalAuthority", "Region", "County", "Country", "lat", "long" },
                SearchMode = SearchMode.All,
                Top = 1,
                QueryType = QueryType.Full
            };

            results = indexClient.Documents.Search<Onspd>(postcode, parameters);

            if (results.Results != null && results.Results.Any())
            {
                return results.Results.First().Document;
            }

            return null;
        }
    }
}
