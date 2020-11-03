using System;

namespace Dfc.CourseDirectory.Web.ViewModels.Locations
{
    public class LocationDeleteConfirmViewModel
    {
       public Guid VenueId { get; set; }
       public string VenueName { get; set; }
       public string PostCode { get; set; }
       public string AddressLine1 { get; set; }
    }
}
