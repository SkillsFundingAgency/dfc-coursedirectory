using System;

namespace Dfc.CourseDirectory.Core.Models
{
    [Flags]
    public enum CourseStatus
    {
        Live = 1,
        Archived = 4,

        // Legacy statuses that are no longer in use:
        //None = 0,
        //Pending = 2,
        //Deleted = 8,
        //BulkUploadPending = 16,
        //BulkUploadReadyToGoLive = 32,
        //APIPending = 64,
        //APIReadyToGoLive = 128,
        //MigrationPending = 256,
        //MigrationReadyToGoLive = 512,
    }
}
