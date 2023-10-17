using System;
using System.IO;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Functions
{
    public class ProcessVenueUpload
    {
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly ILogger<ProcessVenueUpload> _log;

        public ProcessVenueUpload(IFileUploadProcessor fileUploadProcessor,ILogger<ProcessVenueUpload> log)
        {
            _fileUploadProcessor = fileUploadProcessor;
            _log = log;
        }

        [FunctionName(nameof(ProcessVenueUpload))]
        public Task Execute(
            [BlobTrigger("%DataUploadsContainerName%/%VenueUploadsFolderName%/{venueUploadId}.csv")] Stream file,
            Guid venueUploadId)
        {
            _log.LogInformation($"Process Venue Upload venueUploadId{venueUploadId}, filepath: {"%DataUploadsContainerName%/%VenueUploadsFolderName%/{venueUploadId}.csv"}");

            return _fileUploadProcessor.ProcessVenueFile(venueUploadId, file);
        }
    }
}
