using System.Collections.Generic;
using Dfc.CourseDirectory.Services.Models.Venues;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface IVenueSearchResult
    {
        IEnumerable<Venue> Value { get; }
    }
}
