using Dfc.CourseDirectory.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interface.Apprenticeships
{
    public interface ILocation
    {
        Guid id { get; set; } // Cosmos DB id

        LocationType LocationType { get; set; }
        int ProviderUKPRN { get; set; } // As we are trying to inforce unique UKPRN per Provider

        int LocationId { get; set; }
        int ProviderId { get; set; }
        string LocationName { get; set; }
        string AddressLine1 { get; set; }
        string AddressLine2 { get; set; }
        string Town { get; set; }
        string County { get; set; }
        string Postcode { get; set; }
        decimal Latitude { get; set; }
        decimal Longitude { get; set; }
        string Telephone { get; set; }
        string Email { get; set; }
        string Website { get; set; }

        // Standard auditing properties 
        RecordStatus RecordStatus { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedDate { get; set; }
        string UpdatedBy { get; set; }
    }
}
