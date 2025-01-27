using Dfc.CourseDirectory.Core.ReferenceData.Onspd;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Functions
{
    public class AutomatedImportONSPD
    {
        private readonly OnspdDataImporter _onspdDataImporter;
        private readonly ILogger _logger;

        public AutomatedImportONSPD(OnspdDataImporter onspdDataImporter,
            ILogger<AutomatedImportONSPD> logger)
        {
            _onspdDataImporter = onspdDataImporter;
            _logger = logger;
        }
        /*
        [FunctionName("AutomatedImportONSPD")]
        public async Task Run([TimerTrigger("0 0 20 27 * *", RunOnStartup = true)] TimerInfo myTimer)
        {
            await _onspdDataImporter.AutomatedImportData();
        }
        */
        [FunctionName("AutomatedImportONSPD")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request)
        {
            await _onspdDataImporter.AutomatedImportData();
            return new OkObjectResult($"Automated ONSPD data import triggered");
        }
    }
}
