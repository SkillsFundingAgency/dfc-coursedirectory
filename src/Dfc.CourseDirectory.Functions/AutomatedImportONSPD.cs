using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.ReferenceData.Onspd;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
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

        [Function("AutomatedImportONSPD")]
        public async Task<IActionResult> Run([TimerTrigger("0 0 20 27 * *")] TimerInfo myTimer)
        {
            await _onspdDataImporter.AutomatedImportData();
        }
    }
}
