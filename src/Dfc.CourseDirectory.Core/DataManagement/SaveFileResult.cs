using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class SaveFileResult
    {
        public Guid VenueUploadId { get; private set; }
        public SaveFileResultStatus Status { get; private set; }
        public UploadStatus UploadStatus { get; private set; }
        public IReadOnlyCollection<string> MissingHeaders { get; private set; }
        public IReadOnlyCollection<string> MissingLarsRows { get; private set; }
        public IReadOnlyCollection<string> InvalidLarsRows { get; private set; }
        public IReadOnlyCollection<string> ExpiredLarsRows { get; private set; }

        public static SaveFileResult EmptyFile() =>
            new SaveFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                Status = SaveFileResultStatus.EmptyFile
            };

        public static SaveFileResult ExistingFileInFlight() =>
            new SaveFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                Status = SaveFileResultStatus.ExistingFileInFlight
            };

        public static SaveFileResult InvalidFile() =>
            new SaveFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                Status = SaveFileResultStatus.InvalidFile
            };

        public static SaveFileResult InvalidHeader(IEnumerable<string> missingHeaders) =>
            new SaveFileResult()
            {
                MissingHeaders = missingHeaders.ToArray(),
                Status = SaveFileResultStatus.InvalidHeader
            };

        public static SaveFileResult InvalidRows() =>
            new SaveFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                Status = SaveFileResultStatus.InvalidRows
            };

        public static SaveFileResult InvalidLars(IEnumerable<string> missingLars, 
            IEnumerable<string> invalidLars,
            IEnumerable<string> expiredLars) =>
            new SaveFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingLarsRows = missingLars.ToArray(),
                InvalidLarsRows = invalidLars.ToArray(),
                ExpiredLarsRows = expiredLars.ToArray(),
                Status = SaveFileResultStatus.InvalidLars
            };

        public static SaveFileResult InvalidLars(IEnumerable<string> invalidLars) =>
            new SaveFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingLarsRows = Array.Empty<string>(),
                InvalidLarsRows = invalidLars.ToArray(),
                ExpiredLarsRows = expiredLars.ToArray(),
                Status = SaveFileResultStatus.InvalidLars
            };

        public static SaveFileResult Success(Guid venueUploadId, UploadStatus uploadStatus) =>
            new SaveFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingLarsRows = Array.Empty<string>(),
                InvalidLarsRows = Array.Empty<string>(),
                ExpiredLarsRows = Array.Empty<string>(),
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
        InvalidRows,
        EmptyFile,
        ExistingFileInFlight,
        InvalidLars
    }
}
