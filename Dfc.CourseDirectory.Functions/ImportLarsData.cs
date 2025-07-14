using Dfc.CourseDirectory.Core.ReferenceData.Lars;
using Microsoft.Azure.Functions.Worker;

namespace Dfc.CourseDirectory.Functions
{
    public class ImportLarsData
    {
        private readonly LarsDataImporter _dataImporter;

        public ImportLarsData(LarsDataImporter dataImporter)
        {
            _dataImporter = dataImporter;
        }

        [Function("ImportLarsData")]
        public Task Run([TimerTrigger("0 0 4 * * *")] TimerInfo timer) => _dataImporter.ImportData();
    }
}
