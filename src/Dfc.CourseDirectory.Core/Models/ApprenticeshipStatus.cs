using System;

namespace Dfc.CourseDirectory.Core.Models
{
    [Flags]
    public enum ApprenticeshipStatus
    {
        None = 0,
        Live = 1,
        Pending = 2,
        Archived = 4,
        Deleted = 8,
        BulkUploadPending = 16,
        BulkUploadReadyToGoLive = 32,
        APIPending = 64,
        APIReadyToGoLive = 128,
        MigrationPending = 256,
        MigrationReadyToGoLive = 512,
    }
}
