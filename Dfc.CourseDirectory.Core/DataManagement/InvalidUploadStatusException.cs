using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class InvalidUploadStatusException : InvalidStateException
    {
        public InvalidUploadStatusException(UploadStatus status, params UploadStatus[] expectedStatuses)
            : this(status, expectedStatuses.AsEnumerable())
        {
        }

        public InvalidUploadStatusException(UploadStatus status, IEnumerable<UploadStatus> expectedStatuses)
            : base(InvalidStateReason.InvalidUploadStatus)
        {
            Status = status;
            ExpectedStatuses = expectedStatuses.ToArray();

            if (ExpectedStatuses.Length == 0)
            {
                throw new ArgumentException("At least one expected status must be specified.", nameof(expectedStatuses));
            }
        }

        public UploadStatus Status { get; }

        public UploadStatus[] ExpectedStatuses { get; }

        public override string ToString() =>
            $"Invalid upload status: '{Status}' (expecting {string.Join(", ", ExpectedStatuses.Select(s => $"'{s}'"))}.";
    }
}
