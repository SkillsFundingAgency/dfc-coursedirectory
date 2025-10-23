using Dfc.CourseDirectory.Core.ReferenceData.Onspd;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace Dfc.CourseDirectory.Functions
{
    public class AutomatedImportONSPD
    {
        private readonly OnspdDataImporter _onspdDataImporter;

        public AutomatedImportONSPD(OnspdDataImporter onspdDataImporter)
        {
            _onspdDataImporter = onspdDataImporter;
        }

        [Function("AutomatedImportONSPD")]
        public async Task<IActionResult> Run([TimerTrigger("%ImportOnsDataTriggerTimer%", RunOnStartup = true)] TimerInfo timer)
        {
            await _onspdDataImporter.AutomatedDataImport();
            return new OkObjectResult("Automated ONSPD data import triggered");
        }
    }
}
