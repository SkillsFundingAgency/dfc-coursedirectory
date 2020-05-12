using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.ReferenceData.Lars;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class ImportLarsData
    {
        private readonly LarsDataImporter _dataImporter;

        public ImportLarsData(LarsDataImporter dataImporter)
        {
            _dataImporter = dataImporter;
        }

        [FunctionName("ImportLarsData")]
        [NoAutomaticTrigger]
        public Task Run(string input) => _dataImporter.ImportData();
    }
}
