using System;
using System.Collections.Generic;
using System.Text;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Venues;

namespace Dfc.CourseDirectory.Models.Interfaces.Venues
{
    public interface IVenue
    {
        string ID { get; }
        int UKPRN { get; }
        int ProviderID { get; }
        int VenueID { get; }
        string VenueName { get; }
        string ProvVenueID { get; }
        string Address1 { get; }
        string Address2 { get; }
        string Town { get; }
        string County { get; }
        string PostCode { get; }
        decimal Latitude { get; set; }
        decimal Longitude { get; set; }
        VenueStatus Status { get; set; }
        DateTime DateAdded { get; }
        DateTime DateUpdated { get; }
        string UpdatedBy { get; }

        // Apprenticeship related
        int? LocationId { get; set; }
        int? TribalLocationId { get; set; }
        string Telephone { get; set; }
        string Email { get; set; }
        string Website { get; set; }
    }
}


