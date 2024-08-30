using System.Threading.Tasks;
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
        public Task RunNightly(string input) => _onspdDataImporter.ImportData();
    }
}
