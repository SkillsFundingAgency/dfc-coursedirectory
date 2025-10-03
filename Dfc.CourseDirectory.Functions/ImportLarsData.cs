using Dfc.CourseDirectory.Core.ReferenceData.Lars;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> Run([TimerTrigger("%ImportLarsDataTriggerTimer%", RunOnStartup = true)] TimerInfo timer)
        {
            await _dataImporter.ImportData();
            return new OkObjectResult("LARS data import triggered");
        }
    }
}
