using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.Interfaces.VenueService
{
    public interface IUpdatedVenueResult
    {
        string Id { get; }
        string Address1 { get; }
        string Address2 { get; }
        string Town { get; }
        string County { get; }
        string PostCode { get; }

    }
}
