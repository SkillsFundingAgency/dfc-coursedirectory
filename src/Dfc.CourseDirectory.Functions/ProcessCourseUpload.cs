using System;
using System.IO;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Functions
{
    public class ProcessCourseUpload
    {
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly ILogger<ProcessCourseUpload> _log;

        public ProcessCourseUpload(IFileUploadProcessor fileUploadProcessor, ILogger<ProcessCourseUpload> log)
        {
            _fileUploadProcessor = fileUploadProcessor;
            _log = log;
        }

        [FunctionName(nameof(ProcessCourseUpload))]
        public Task Execute(
            [BlobTrigger("%DataUploadsContainerName%/%CourseUploadsFolderName%/{courseUploadId}.csv")] Stream file,
            Guid courseUploadId)
        {
            _log.LogInformation($"Process Course Upload courseUploadId{courseUploadId}, filepath: {"%DataUploadsContainerName%/%CourseUploadsFolderName%/{courseUploadId}.csv"}");
            return _fileUploadProcessor.ProcessCourseFile(courseUploadId, file);
        }
    }
}
