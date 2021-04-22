using System;
using System.IO;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class ProcessVenueUpload
    {
        private readonly IFileUploadProcessor _fileUploadProcessor;

        public ProcessVenueUpload(IFileUploadProcessor fileUploadProcessor)
        {
            _fileUploadProcessor = fileUploadProcessor;
        }

        [FunctionName(nameof(ProcessVenueUpload))]
        public Task Execute(
            [BlobTrigger("%DataUploadsContainerName%/%VenueUploadsFolderName%/{venueUploadId}.csv")] Stream file,
            Guid venueUploadId)
        {
            return _fileUploadProcessor.ProcessVenueFile(venueUploadId, file);
        }
    }
}
