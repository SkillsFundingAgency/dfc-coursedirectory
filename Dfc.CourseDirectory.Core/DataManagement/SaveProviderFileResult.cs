using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public sealed class SaveProviderFileResult
    {
        public Guid ProviderUploadId { get; private set; }
        public SaveProviderFileResultStatus Status { get; private set; }
        public UploadStatus UploadStatus { get; private set; }
        public IReadOnlyCollection<string> MissingHeaders { get; private set; }
        public static SaveProviderFileResult EmptyFile() =>
            new SaveProviderFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                
                Status = SaveProviderFileResultStatus.EmptyFile
            };
        public static SaveProviderFileResult ExistingFileInFlight() =>
           new SaveProviderFileResult()
           {
               MissingHeaders = Array.Empty<string>(),
               Status = SaveProviderFileResultStatus.ExistingFileInFlight
           };

        public static SaveProviderFileResult InvalidFile() =>
            new SaveProviderFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                Status = SaveProviderFileResultStatus.InvalidFile
            };

        public static SaveProviderFileResult InvalidHeader(IEnumerable<string> missingHeaders) =>
            new SaveProviderFileResult()
            {
                MissingHeaders = missingHeaders.ToArray(),
                Status = SaveProviderFileResultStatus.InvalidHeader
            };
        public static SaveProviderFileResult Success(Guid providerUploadId, UploadStatus uploadStatus) =>
          new SaveProviderFileResult()
          {
              MissingHeaders = Array.Empty<string>(),
              Status = SaveProviderFileResultStatus.Success,
              UploadStatus = uploadStatus,
              ProviderUploadId = providerUploadId
          };
    }
    public enum SaveProviderFileResultStatus
    {
        Success,
        InvalidFile,
        InvalidHeader,
        InvalidRows,
        EmptyFile,
        ExistingFileInFlight
    }
}
