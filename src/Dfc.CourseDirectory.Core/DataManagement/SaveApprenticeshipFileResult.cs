using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public sealed class SaveApprenticeshipFileResult
    {
        public Guid ApprenticeshipUploadId { get; private set; }
        public SaveApprenticeshipFileResultStatus Status { get; private set; }
        public UploadStatus UploadStatus { get; private set; }
        public IReadOnlyCollection<string> MissingHeaders { get; private set; }
        public IReadOnlyCollection<int> MissingStandardRows { get; private set; }
        public IReadOnlyCollection<(int StandardCode, int StandardVersion, int RowNumber)> InvalidStandardRows { get; private set; }

        public static SaveApprenticeshipFileResult EmptyFile() =>
            new SaveApprenticeshipFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingStandardRows = Array.Empty<int>(),
                InvalidStandardRows = Array.Empty<(int StandardCode, int StandardVersion, int RowNumber)>(),
                Status = SaveApprenticeshipFileResultStatus.EmptyFile
            };

        public static SaveApprenticeshipFileResult ExistingFileInFlight() =>
            new SaveApprenticeshipFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingStandardRows = Array.Empty<int>(),
                InvalidStandardRows = Array.Empty<(int StandardCode, int StandardVersion, int RowNumber)>(),
                Status = SaveApprenticeshipFileResultStatus.ExistingFileInFlight
            };

        public static SaveApprenticeshipFileResult InvalidFile() =>
            new SaveApprenticeshipFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingStandardRows = Array.Empty<int>(),
                InvalidStandardRows = Array.Empty<(int StandardCode, int StandardVersion, int RowNumber)>(),
                Status = SaveApprenticeshipFileResultStatus.InvalidFile
            };

        public static SaveApprenticeshipFileResult InvalidHeader(IEnumerable<string> missingHeaders) =>
            new SaveApprenticeshipFileResult()
            {
                MissingHeaders = missingHeaders.ToArray(),
                MissingStandardRows = Array.Empty<int>(),
                InvalidStandardRows = Array.Empty<(int StandardCode, int StandardVersion, int RowNumber)>(),
                Status = SaveApprenticeshipFileResultStatus.InvalidHeader
            };

        public static SaveApprenticeshipFileResult InvalidRows() =>
            new SaveApprenticeshipFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingStandardRows = Array.Empty<int>(),
                InvalidStandardRows = Array.Empty<(int StandardCode, int StandardVersion, int RowNumber)>(),
                Status = SaveApprenticeshipFileResultStatus.InvalidRows
            };

        public static SaveApprenticeshipFileResult InvalidStandards(
                IEnumerable<int> missingStandards, 
                IEnumerable<(int StandardCode, int StandardVersion, int RowNumber)> invalidStandards) =>
            new SaveApprenticeshipFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingStandardRows = missingStandards.ToArray(),
                InvalidStandardRows = invalidStandards.ToArray(),
                Status = SaveApprenticeshipFileResultStatus.InvalidStandards
            };

        public static SaveApprenticeshipFileResult Success(Guid apprenticeshipUploadId, UploadStatus uploadStatus) =>
            new SaveApprenticeshipFileResult()
            {
                MissingHeaders = Array.Empty<string>(),
                MissingStandardRows = Array.Empty<int>(),
                InvalidStandardRows = Array.Empty<(int StandardCode, int StandardVersion, int RowNumber)>(),
                Status = SaveApprenticeshipFileResultStatus.Success,
                UploadStatus = uploadStatus,
                ApprenticeshipUploadId = apprenticeshipUploadId
            };
    }

    public enum SaveApprenticeshipFileResultStatus
    {
        Success,
        InvalidFile,
        InvalidHeader,
        InvalidRows,
        EmptyFile,
        ExistingFileInFlight,
        InvalidStandards
    }
}
