using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.Interfaces.VenueService
{
    public interface IGetVenueByIdSettings
    {
        string ApiUrl { get; }
        string ApiKey { get; }
    }
}
