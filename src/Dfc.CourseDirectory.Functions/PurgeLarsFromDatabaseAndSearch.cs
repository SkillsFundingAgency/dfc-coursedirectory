using System;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Functions.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Functions
{
    public class PurgeLarsFromDatabaseAndSearch
    {
        private const string LarsSearchIndexName = "lars";

        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly SearchClient _searchClient;

        public PurgeLarsFromDatabaseAndSearch(ISqlQueryDispatcher sqlQueryDispatcher, Func<SearchClientSettings, SearchClient> searchClientFactory, IConfiguration configuration)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher ?? throw new ArgumentNullException(nameof(sqlQueryDispatcher));
            
            if (searchClientFactory == null)
            {
                throw new ArgumentNullException(nameof(searchClientFactory));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _searchClient = searchClientFactory(new SearchClientSettings(
                configuration.GetValue<string>("AzureSearchUrl"),
                configuration.GetValue<string>("AzureSearchAdminKey"),
                LarsSearchIndexName));
        }

        [FunctionName("PurgeLarsFromDatabaseAndSearch")]
        [NoAutomaticTrigger]
        public async Task Run(string learnAimRef, ILogger log)
        {
            if (string.IsNullOrWhiteSpace(learnAimRef))
            {
                log.LogError($"{nameof(PurgeLarsFromDatabaseAndSearch)} failed with invalid {nameof(learnAimRef)} {{{nameof(learnAimRef)}}}.", learnAimRef);
            }

            log.LogInformation($"{nameof(PurgeLarsFromDatabaseAndSearch)} invoked for {nameof(learnAimRef)} {{{nameof(learnAimRef)}}}.", learnAimRef);

            var deletedResult = await _sqlQueryDispatcher.ExecuteQuery(new DeleteLarsLearningDelivery(learnAimRef));

            deletedResult.Switch(
                deletedLearnAimRef => log.LogInformation($"{nameof(PurgeLarsFromDatabaseAndSearch)} successfully deleted {nameof(learnAimRef)}: {{{nameof(learnAimRef)}}} from database.", learnAimRef),
                _ => log.LogWarning($"{nameof(PurgeLarsFromDatabaseAndSearch)} failed to find {nameof(learnAimRef)} {{{nameof(learnAimRef)}}} in database.", learnAimRef));

            try
            {
                await _searchClient.GetDocumentAsync<dynamic>(learnAimRef);
                await _searchClient.DeleteDocumentsAsync("LearnAimRef", new[] { learnAimRef }, new IndexDocumentsOptions { ThrowOnAnyError = true });

                log.LogInformation($"{nameof(PurgeLarsFromDatabaseAndSearch)} successfully deleted {nameof(learnAimRef)} {{{nameof(learnAimRef)}}} from search index.", learnAimRef);
            }
            catch (RequestFailedException ex)
                when (ex.Status == StatusCodes.Status404NotFound)
            {
                log.LogWarning($"{nameof(PurgeLarsFromDatabaseAndSearch)} failed to find {nameof(learnAimRef)} {{{nameof(learnAimRef)}}} in search.", learnAimRef);
            }
        }
    }
}