using Dfc.CourseDirectory.Services.Interfaces;

namespace Dfc.CourseDirectory.Services
{
    public class VenueAddSettings : IVenueAddSettings
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
    }
}
