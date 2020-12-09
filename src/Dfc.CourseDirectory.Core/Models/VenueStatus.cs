namespace Dfc.CourseDirectory.Core.Models
{
    public enum VenueStatus
    {
        Live = 1,

        /// <summary>
        /// App doesn't support Pending venues but we have some data with this status
        /// </summary>
        Pending = 2,

        Archived = 4
    }
}
