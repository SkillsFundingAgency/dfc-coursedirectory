using Dfc.CourseDirectory.Core.DataManagement;
using Microsoft.Azure.Functions.Worker;

namespace Dfc.CourseDirectory.Functions
{
    public class ProcessVenueUpload
    {
        private readonly IFileUploadProcessor _fileUploadProcessor;

        public ProcessVenueUpload(IFileUploadProcessor fileUploadProcessor)
        {
            _fileUploadProcessor = fileUploadProcessor;
        }

        [Function(nameof(ProcessVenueUpload))]
        public Task Run(
            [BlobTrigger("%DataUploadsContainerName%/%VenueUploadsFolderName%/{venueUploadId}.csv")] Stream file,
            Guid venueUploadId)
        {
            return _fileUploadProcessor.ProcessVenueFile(venueUploadId, file);
        }
    }
}
