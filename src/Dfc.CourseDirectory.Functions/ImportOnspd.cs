using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.ReferenceData.Onspd;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class ImportOnspd
    {
        private readonly OnspdDataImporter _onspdDataImporter;

        public ImportOnspd(OnspdDataImporter onspdDataImporter)
        {
            _onspdDataImporter = onspdDataImporter;
        }

        [FunctionName("ImportOnspd")]
        [Singleton]
        [NoAutomaticTrigger]
        public Task RunNightly(string input) => _onspdDataImporter.ImportData();
    }
}
