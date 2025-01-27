using Dfc.CourseDirectory.Core.ReferenceData.Lars;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class ImportLarsData
    {
        private readonly LarsDataImporter _dataImporter;

        public ImportLarsData(LarsDataImporter dataImporter)
        {
            _dataImporter = dataImporter;
        }

        [FunctionName("ImportLarsData")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request)
        {
            await _dataImporter.ImportData();
            return new OkObjectResult($"LARS data import triggered");
        }
    }
}
