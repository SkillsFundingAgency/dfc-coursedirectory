using Dfc.CourseDirectory.Core.ReferenceData.Onspd;
using Microsoft.Azure.Functions.Worker;

namespace Dfc.CourseDirectory.Functions
{
    public class ImportOnspd
    {
        private readonly OnspdDataImporter _onspdDataImporter;

        public ImportOnspd(OnspdDataImporter onspdDataImporter)
        {
            _onspdDataImporter = onspdDataImporter;
        }

        [Function("ImportOnspd")]
        public Task Run([TimerTrigger("0 2 * * * *")] TimerInfo timer) => _onspdDataImporter.ImportData();
    }
}
