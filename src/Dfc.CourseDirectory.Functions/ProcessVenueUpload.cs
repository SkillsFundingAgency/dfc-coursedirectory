using System;
using System.IO;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class ProcessVenueUpload
    {
        private readonly IVenueUploadProcessor _venueUploadProcessor;

        public ProcessVenueUpload(IVenueUploadProcessor venueUploadProcessor)
        {
            _venueUploadProcessor = venueUploadProcessor;
        }

        [FunctionName(nameof(ProcessVenueUpload))]
        public Task Execute(
            [BlobTrigger("%DataUploadsContainerName%/%VenueUploadsFolderName%/{venueUploadId}.csv")] Stream file,
            Guid venueUploadId)
        {
            return _venueUploadProcessor.ProcessFile(venueUploadId, file);
        }
    }
}
