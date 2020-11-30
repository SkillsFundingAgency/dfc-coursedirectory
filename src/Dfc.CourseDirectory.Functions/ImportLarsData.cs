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
        [Singleton]
        [Disable]
        public Task Run([TimerTrigger("0 0 4 * * *")] TimerInfo timer) => _dataImporter.ImportData();
    }
}
