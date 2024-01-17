using System;
using System.IO;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class ProcessCourseUpload
    {
        private readonly IFileUploadProcessor _fileUploadProcessor;

        public ProcessCourseUpload(IFileUploadProcessor fileUploadProcessor)
        {
            _fileUploadProcessor = fileUploadProcessor;
        }

        [FunctionName(nameof(ProcessCourseUpload))]
        public Task Execute(
            [BlobTrigger("%DataUploadsContainerName%/%CourseUploadsFolderName%/{courseUploadId}.csv")] Stream file,
            Guid courseUploadId)
        {
            return _fileUploadProcessor.ProcessCourseFile(courseUploadId, file);
        }
    }
}
