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
        public async Task Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request)
        {
            _logger.LogInformation("{FunctionName} function has been invoked", nameof(ManualImportONSPD));
            string filename = request.Query["filename"];

            if (string.IsNullOrEmpty(filename))
            {
                _logger.LogWarning("Please enter the target CSV filename with its file extension using the parameter 'filename'.");
            }

            if (!filename!.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("The filename must include a '.csv' file extension");
            }

            try
            {
                await _onspdDataImporter.ManualDataImport(filename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while importing ONSPD file: {FileName}", filename);
            }

            _logger.LogInformation("Successfully manually imported the ONSPD for file: {FileName}", filename);
            _logger.LogInformation("{FunctionName} function has finished invoking", nameof(ManualImportONSPD));
        }

        [Function("BlobImportONSPD")]
        public async Task Run(
            [BlobTrigger("onspd/{name}", Connection = "BlobStorageSettings__ConnectionString")]
            Stream blobStream,
            string name)
        {
            _logger.LogInformation("{FunctionName} function has been invoked for blob: {FileName}", nameof(ManualImportONSPD), name);

            try
            {
                await _onspdDataImporter.BlobImportONSPD(blobStream, name);

                _logger.LogInformation("{FunctionName} has finished processing file: {FileName}", "BlobImportONSPD", name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while importing ONSPD file from blob: {FileName}. Error: {ErrorMessage}", name, ex.Message);
            }
        }
    }
}
