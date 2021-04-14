using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class SaveFileResult
    {
        public Guid VenueUploadId { get; private set; }
        public SaveFileResultStatus Status { get; private set; }
        public UploadStatus UploadStatus { get; private set; }
        public IReadOnlyCollection<string> MissingHeaders { get; private set; }

        public static SaveFileResult EmptyFile() =>
            new SaveFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                Status = SaveFileResultStatus.EmptyFile
            };

        public static SaveFileResult InvalidFile() =>
            new SaveFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                Status = SaveFileResultStatus.InvalidFile
            };

        public static SaveFileResult Success(Guid venueUploadId, UploadStatus uploadStatus) =>
            new SaveFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                Status = SaveFileResultStatus.Success,
                UploadStatus = uploadStatus,
                VenueUploadId = venueUploadId
            };
    }

    public enum SaveFileResultStatus
    {
        Success,
        InvalidFile,
        InvalidHeader,
        EmptyFile,
    }
}
