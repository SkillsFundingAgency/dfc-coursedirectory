using System;
using System.Collections.Generic;
using System.Text;
using Dfc.CourseDirectory.Models.Models.Venues;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface IVenueSearchResult
    {
        IEnumerable<Venue> Value { get; }
    }
}
