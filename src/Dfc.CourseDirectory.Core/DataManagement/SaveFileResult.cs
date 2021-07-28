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
        public IReadOnlyCollection<int> MissingLearnAimRefs { get; private set; }
        public IReadOnlyCollection<(string LearnAimRef, int RowNumber)> InvalidLearnAimRefs { get; private set; }
        public IReadOnlyCollection<(string LearnAimRef, int RowNumber)> ExpiredLearnAimRefs { get; private set; }

        public static SaveFileResult EmptyFile() =>
            new SaveFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingLearnAimRefs = Array.Empty<int>(),
                InvalidLearnAimRefs = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                ExpiredLearnAimRefs = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                Status = SaveFileResultStatus.EmptyFile
            };

        public static SaveFileResult ExistingFileInFlight() =>
            new SaveFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingLearnAimRefs = Array.Empty<int>(),
                InvalidLearnAimRefs = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                ExpiredLearnAimRefs = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                Status = SaveFileResultStatus.ExistingFileInFlight
            };

        public static SaveFileResult InvalidFile() =>
            new SaveFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingLearnAimRefs = Array.Empty<int>(),
                InvalidLearnAimRefs = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                ExpiredLearnAimRefs = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                Status = SaveFileResultStatus.InvalidFile
            };

        public static SaveFileResult InvalidHeader(IEnumerable<string> missingHeaders) =>
            new SaveFileResult()
            {
                MissingHeaders = missingHeaders.ToArray(),
                MissingLearnAimRefs = Array.Empty<int>(),
                InvalidLearnAimRefs = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                ExpiredLearnAimRefs = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                Status = SaveFileResultStatus.InvalidHeader
            };

        public static SaveFileResult InvalidRows() =>
            new SaveFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingLearnAimRefs = Array.Empty<int>(),
                InvalidLearnAimRefs = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                ExpiredLearnAimRefs = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                Status = SaveFileResultStatus.InvalidRows
            };

        public static SaveFileResult InvalidLars(
                IEnumerable<int> missingLearnAimRefs, 
                IEnumerable<(string LearnAimRef, int RowNumber)> invalidLearnAimRefs,
                IEnumerable<(string LearnAimRef, int RowNumber)> expiredLearnAimRefs) =>
            new SaveFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingLearnAimRefs = missingLearnAimRefs.ToArray(),
                InvalidLearnAimRefs = invalidLearnAimRefs.ToArray(),
                ExpiredLearnAimRefs = expiredLearnAimRefs.ToArray(),
                Status = SaveFileResultStatus.InvalidLars
            };

        public static SaveFileResult Success(Guid venueUploadId, UploadStatus uploadStatus) =>
            new SaveFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingLearnAimRefs = Array.Empty<int>(),
                InvalidLearnAimRefs = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                ExpiredLearnAimRefs = Array.Empty<(string LearnAimRef, int RowNumber)>(),
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
