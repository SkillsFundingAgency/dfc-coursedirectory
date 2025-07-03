using Dfc.CourseDirectory.Core.ReferenceData.Onspd;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Functions
{
    public class ManualImportONSPD
    {
        private readonly OnspdDataImporter _onspdDataImporter;
        private readonly ILogger<ManualImportONSPD> _logger;

        public ManualImportONSPD(OnspdDataImporter onspdDataImporter, ILogger<ManualImportONSPD> logger)
        {
            _onspdDataImporter = onspdDataImporter;
            _logger = logger;
        }

        [Function("ManualImportONSPD")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request)

        {
            _logger.LogInformation("{FunctionName} function has been invoked", nameof(ManualImportONSPD));
            string filename = request.Query["filename"];

            if (string.IsNullOrEmpty(filename))
            {
                return new BadRequestObjectResult(
                    "Please enter the target CSV filename with its file extension using the parameter 'filename'.");
            }

            await _onspdDataImporter.ManualDataImport(filename);

            _logger.LogInformation("{FunctionName} function has finished invoking", nameof(ManualImportONSPD));
            return new OkObjectResult($"Successfully manually imported the ONSPD for file: {filename}");
        }
    }
}
