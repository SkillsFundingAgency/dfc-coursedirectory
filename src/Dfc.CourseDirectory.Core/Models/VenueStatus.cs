namespace Dfc.CourseDirectory.Core.Models
{
    public enum VenueStatus
    {
        Live = 1,
        Pending = 2,  // App doesn't support Pending venues but we have some data with this status
        Archived = 4
    }
}
