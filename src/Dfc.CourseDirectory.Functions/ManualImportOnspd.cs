using Dfc.CourseDirectory.Core.ReferenceData.Onspd;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Functions
{
    public class ManualImportONSPD
    {
        private readonly OnspdDataImporter _onspdDataImporter;
        private readonly ILogger<ManualImportONSPD> _logger;

        public ManualImportONSPD(
            OnspdDataImporter onspdDataImporter,
            ILogger<ManualImportONSPD> logger)
        {
            _onspdDataImporter = onspdDataImporter;
            _logger = logger;
        }

        [Function("ManualImportONSPD")]
        public async Task Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request)

        {
            string filename = request.Query["filename"];

            if (string.IsNullOrEmpty(filename))
            {
                _logger.LogError("Bad request: please indicate the target .csv filename using the filename parameter.");
            }
            else
            {
                _logger.LogInformation($"Manual data import triggered for filename: {filename}");
                await _onspdDataImporter.ManualDataImport(filename);
            }
        }
    }
}
