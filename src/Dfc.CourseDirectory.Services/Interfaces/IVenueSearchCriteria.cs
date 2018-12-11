using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface IVenueSearchCriteria
    {
        string Search { get; }

        string NewAddressId { get; }
    }
}
