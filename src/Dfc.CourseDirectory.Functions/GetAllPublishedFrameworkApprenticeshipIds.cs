using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Functions
{
    public class GetAllPublishedFrameworkApprenticeshipIds
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public GetAllPublishedFrameworkApprenticeshipIds(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher ?? throw new ArgumentNullException(nameof(cosmosDbQueryDispatcher));
        }

        [FunctionName("GetAllPublishedFrameworkApprenticeshipIds")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req, ILogger log)
        {
            var results = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetApprenticeships(a => a.FrameworkId != null));

            return new OkObjectResult(results.Values.Select(f => f.Id));
        }
    }
}
