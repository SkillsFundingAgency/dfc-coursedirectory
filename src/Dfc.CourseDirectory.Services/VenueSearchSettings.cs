using Dfc.CourseDirectory.Services.Interfaces;

namespace Dfc.CourseDirectory.Services
{
    public class VenueSearchSettings : IVenueSearchSettings
    {
        public string ApiUrl { get; set; }
        public string ApiVersion { get; set; }
        public string ApiKey { get; set; }
    }
}
