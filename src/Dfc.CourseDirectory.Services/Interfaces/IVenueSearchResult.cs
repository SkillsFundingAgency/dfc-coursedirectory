using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface IVenueSearchResult
    {
        IEnumerable<VenueSearchResultItem> Value { get; }
    }
}
