using Dfc.CourseDirectory.Core.ReferenceData.Onspd;
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
        public async Task Run([TimerTrigger("0 0 20 27 * *", RunOnStartup = true)] TimerInfo myTimer)
        {
            await _onspdDataImporter.AutomatedImportData();
        }
    }
}
