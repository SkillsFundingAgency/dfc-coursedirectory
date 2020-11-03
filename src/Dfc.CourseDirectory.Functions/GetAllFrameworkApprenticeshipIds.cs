using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Functions
{
    public class GetAllFrameworkApprenticeshipIds
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public GetAllFrameworkApprenticeshipIds(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher ?? throw new ArgumentNullException(nameof(cosmosDbQueryDispatcher));
        }

        [FunctionName("GetAllFrameworkApprenticeshipIds")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req, ILogger log)
        {
            var results = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetApprenticeships(a => a.ApprenticeshipType == ApprenticeshipType.FrameworkCode));

            return new OkObjectResult(results.Values.Select(f => f.Id));
        }
    }
}
