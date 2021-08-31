using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public sealed class SaveCourseFileResult
    {
        public Guid CourseUploadId { get; private set; }
        public SaveCourseFileResultStatus Status { get; private set; }
        public UploadStatus UploadStatus { get; private set; }
        public IReadOnlyCollection<string> MissingHeaders { get; private set; }
        public IReadOnlyCollection<int> MissingLearnAimRefRows { get; private set; }
        public IReadOnlyCollection<(string LearnAimRef, int RowNumber)> InvalidLearnAimRefRows { get; private set; }
        public IReadOnlyCollection<(string LearnAimRef, int RowNumber)> ExpiredLearnAimRefRows { get; private set; }

        public static SaveCourseFileResult EmptyFile() =>
            new SaveCourseFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingLearnAimRefRows = Array.Empty<int>(),
                InvalidLearnAimRefRows = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                ExpiredLearnAimRefRows = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                Status = SaveCourseFileResultStatus.EmptyFile
            };

        public static SaveCourseFileResult ExistingFileInFlight() =>
            new SaveCourseFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingLearnAimRefRows = Array.Empty<int>(),
                InvalidLearnAimRefRows = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                ExpiredLearnAimRefRows = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                Status = SaveCourseFileResultStatus.ExistingFileInFlight
            };

        public static SaveCourseFileResult InvalidFile() =>
            new SaveCourseFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingLearnAimRefRows = Array.Empty<int>(),
                InvalidLearnAimRefRows = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                ExpiredLearnAimRefRows = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                Status = SaveCourseFileResultStatus.InvalidFile
            };

        public static SaveCourseFileResult InvalidHeader(IEnumerable<string> missingHeaders) =>
            new SaveCourseFileResult()
            {
                MissingHeaders = missingHeaders.ToArray(),
                MissingLearnAimRefRows = Array.Empty<int>(),
                InvalidLearnAimRefRows = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                ExpiredLearnAimRefRows = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                Status = SaveCourseFileResultStatus.InvalidHeader
            };

        public static SaveCourseFileResult InvalidRows() =>
            new SaveCourseFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingLearnAimRefRows = Array.Empty<int>(),
                InvalidLearnAimRefRows = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                ExpiredLearnAimRefRows = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                Status = SaveCourseFileResultStatus.InvalidRows
            };

        public static SaveCourseFileResult InvalidLars(
                IEnumerable<int> missingLearnAimRefs, 
                IEnumerable<(string LearnAimRef, int RowNumber)> invalidLearnAimRefs,
                IEnumerable<(string LearnAimRef, int RowNumber)> expiredLearnAimRefs) =>
            new SaveCourseFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingLearnAimRefRows = missingLearnAimRefs.ToArray(),
                InvalidLearnAimRefRows = invalidLearnAimRefs.ToArray(),
                ExpiredLearnAimRefRows = expiredLearnAimRefs.ToArray(),
                Status = SaveCourseFileResultStatus.InvalidLars
            };

        public static SaveCourseFileResult Success(Guid courseUploadId, UploadStatus uploadStatus) =>
            new SaveCourseFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingLearnAimRefRows = Array.Empty<int>(),
                InvalidLearnAimRefRows = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                ExpiredLearnAimRefRows = Array.Empty<(string LearnAimRef, int RowNumber)>(),
                Status = SaveCourseFileResultStatus.Success,
                UploadStatus = uploadStatus,
                CourseUploadId = courseUploadId
            };
    }

    public enum SaveCourseFileResultStatus
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
