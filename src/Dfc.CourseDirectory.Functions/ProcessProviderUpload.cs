using Dfc.CourseDirectory.Core.DataManagement;
using Microsoft.Azure.Functions.Worker;

namespace Dfc.CourseDirectory.Functions
{
    public class ProcessProviderUpload
    {
        private readonly IFileUploadProcessor _fileUploadProcessor;

        public ProcessProviderUpload(IFileUploadProcessor fileUploadProcessor)
        {
            _fileUploadProcessor = fileUploadProcessor;
        }

        [Function(nameof(ProcessProviderUpload))]
        public Task Run(
            [BlobTrigger("%DataUploadsContainerName%/%ProviderUploadsFolderName%/{courseUploadId}.csv")] Stream file,
            Guid providerUploadId)
        {
            return _fileUploadProcessor.ProcessProviderFile(providerUploadId, file);
        }
    }
}
