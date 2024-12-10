using Dfc.CourseDirectory.Core.ReferenceData.Onspd;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;

namespace Dfc.CourseDirectory.Functions
{
    public class ManualImportONSPD
    {
        private readonly OnspdDataImporter _onspdDataImporter;

        public ManualImportONSPD(OnspdDataImporter onspdDataImporter)
        {
            _onspdDataImporter = onspdDataImporter;
        }

        [Function("ManualImportONSPD")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request)

        {
            string filename = request.Query["filename"];

            if (string.IsNullOrEmpty(filename))
            {
                return new BadRequestObjectResult("Bad request: please indicate the target .csv filename using the filename parameter.");
            }

            await _onspdDataImporter.ManualDataImport(filename);

            return new OkObjectResult($"Manual data import triggered for filename: {filename}");
        }
    }
}
