using Dfc.CourseDirectory.Core.ReferenceData.Onspd;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

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
        public async Task Run([TimerTrigger("0 0 20 27 * *")] TimerInfo myTimer)
        {
            await _onspdDataImporter.AutomatedImportData();
        }
    }
}
