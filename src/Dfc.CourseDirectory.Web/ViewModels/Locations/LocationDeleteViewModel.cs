using System;
using Dfc.CourseDirectory.Services.Enums;

namespace Dfc.CourseDirectory.Web.ViewModels.Locations
{
    public class LocationDeleteViewModel
    {
        public Guid VenueId { get; set; }
        public string VenueName { get; set; }
        public string PostCode { get; set; }
        public string AddressLine1 { get; set; }
        public LocationDelete LocationDelete { get; set; }
    }
}
