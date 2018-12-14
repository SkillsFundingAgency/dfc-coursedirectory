using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.Interfaces.VenueService
{
    public interface IVenueServiceSettings
    {
        string ApiUrl { get; }
        string ApiKey { get; }
    }
}
