using System;
using System.IO;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class ProcessApprenticeshipUpload
    {
        private readonly IFileUploadProcessor _fileUploadProcessor;

        public ProcessApprenticeshipUpload(IFileUploadProcessor fileUploadProcessor)
        {
            _fileUploadProcessor = fileUploadProcessor;
        }

        [FunctionName(nameof(ProcessApprenticeshipUpload))]
        public Task Execute(
            [BlobTrigger("%DataUploadsContainerName%/%ApprenticeshipUploadsFolderName%/{apprenticeshipUploadId}.csv")] Stream file,
            Guid apprenticeshipUploadId)
        {
            return _fileUploadProcessor.ProcessApprenticeshipFile(apprenticeshipUploadId, file);
        }
    }
}
