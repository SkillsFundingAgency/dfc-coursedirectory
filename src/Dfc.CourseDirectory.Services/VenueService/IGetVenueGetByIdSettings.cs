using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;

namespace Dfc.CourseDirectory.Services.VenueService
{
    public class GetVenueByIdSettings : IGetVenueByIdSettings
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
    }
}
