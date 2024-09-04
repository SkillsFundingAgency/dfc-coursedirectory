using Dfc.CourseDirectory.Core.DataManagement;
using Microsoft.Azure.Functions.Worker;

namespace Dfc.CourseDirectory.Functions
{
    public class ProcessCourseUpload
    {
        private readonly IFileUploadProcessor _fileUploadProcessor;

        public ProcessCourseUpload(IFileUploadProcessor fileUploadProcessor)
        {
            _fileUploadProcessor = fileUploadProcessor;
        }

        [Function(nameof(ProcessCourseUpload))]
        public Task Execute(
            [BlobTrigger("%DataUploadsContainerName%/%CourseUploadsFolderName%/{courseUploadId}.csv")] Stream file,
            Guid courseUploadId)
        {
            return _fileUploadProcessor.ProcessCourseFile(courseUploadId, file);
        }
    }
}
