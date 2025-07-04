using Dfc.CourseDirectory.Core.ReferenceData.Onspd;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Functions
{
    public class BlobImportONSPD
    {
        private readonly OnspdDataImporter _onspdDataImporter;
        private readonly ILogger<BlobImportONSPD> _logger;

        public BlobImportONSPD(OnspdDataImporter onspdDataImporter, ILogger<BlobImportONSPD> logger)
        {
            _onspdDataImporter = onspdDataImporter;
            _logger = logger;
        }

        [Function("BlobImportONSPD")]
        public async Task Run(
            [BlobTrigger("onspd/{fileName}.csv")]
            Stream blobStream,
            string fileName)
        {
            _logger.LogInformation("{FunctionName} function has been invoked for blob: {FileName}", nameof(ManualImportONSPD), fileName);

            try
            {
                await _onspdDataImporter.BlobImportONSPD(blobStream, fileName);

                _logger.LogInformation("{FunctionName} has finished processing file: {FileName}", "BlobImportONSPD", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while importing ONSPD file from blob: {FileName}. Error: {ErrorMessage}", fileName, ex.Message);
            }
        }
    }
}
