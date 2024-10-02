using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.ReferenceData.Onspd;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class ManualImportONSPD
    {
        private readonly OnspdDataImporter _onspdDataImporter;

        public ManualImportONSPD(OnspdDataImporter onspdDataImporter)
        {
            _onspdDataImporter = onspdDataImporter;
        }

        [FunctionName("ManualImportONSPD")]
        [Singleton]
        [NoAutomaticTrigger]
        public Task Run(string filename) => _onspdDataImporter.ManualDataImport(filename);
    }
}
