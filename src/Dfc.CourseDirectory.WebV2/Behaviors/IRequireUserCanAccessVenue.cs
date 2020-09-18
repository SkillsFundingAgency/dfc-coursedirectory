using System;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public interface IRequireUserCanAccessVenue<in TRequest>
    {
        Guid GetVenueId(TRequest request);
    }
}
