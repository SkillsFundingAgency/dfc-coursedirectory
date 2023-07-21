using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.ReferenceData.Onspd;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
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

        [FunctionName("AutomatedImportONSPD")]
        public async Task Run([TimerTrigger("0 0 20 28-31 * *", RunOnStartup = true)] TimerInfo myTimer)
        {
            await _onspdDataImporter.AutomatedImportData();
        }
    }
}
