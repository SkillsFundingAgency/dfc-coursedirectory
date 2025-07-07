using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public sealed class SaveVenueFileResult
    {
        public Guid VenueUploadId { get; private set; }
        public SaveVenueFileResultStatus Status { get; private set; }
        public UploadStatus UploadStatus { get; private set; }
        public IReadOnlyCollection<string> MissingHeaders { get; private set; }

        public static SaveVenueFileResult EmptyFile() =>
            new SaveVenueFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                Status = SaveVenueFileResultStatus.EmptyFile
            };

        public static SaveVenueFileResult ExistingFileInFlight() =>
            new SaveVenueFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                Status = SaveVenueFileResultStatus.ExistingFileInFlight
            };

        public static SaveVenueFileResult InvalidFile() =>
            new SaveVenueFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                Status = SaveVenueFileResultStatus.InvalidFile
            };

        public static SaveVenueFileResult InvalidHeader(IEnumerable<string> missingHeaders) =>
            new SaveVenueFileResult()
            {
                MissingHeaders = missingHeaders.ToArray(),
                Status = SaveVenueFileResultStatus.InvalidHeader
            };

        public static SaveVenueFileResult InvalidRows() =>
            new SaveVenueFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                Status = SaveVenueFileResultStatus.InvalidRows
            };

        public static SaveVenueFileResult Success(Guid venueUploadId, UploadStatus uploadStatus) =>
            new SaveVenueFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                Status = SaveVenueFileResultStatus.Success,
                UploadStatus = uploadStatus,
                VenueUploadId = venueUploadId
            };
    }

    public enum SaveVenueFileResultStatus
    {
        Success,
        InvalidFile,
        InvalidHeader,
        InvalidRows,
        EmptyFile,
        ExistingFileInFlight
    }
}
