using System;

namespace Dfc.CourseDirectory.Core.Models
{
    [Flags]
    public enum ApprenticeshipStatus
    {
        Live = 1,
        Pending = 2,  // Used for QA
        Archived = 4,

        // Legacy statuses that are no longer in use:
        //None = 0,
        //Deleted = 8,
        //BulkUploadPending = 16,
        //BulkUploadReadyToGoLive = 32,
        //APIPending = 64,
        //APIReadyToGoLive = 128,
        //MigrationPending = 256,
        //MigrationReadyToGoLive = 512,
    }
}
