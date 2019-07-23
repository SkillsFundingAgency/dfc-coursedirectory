using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interface.Apprenticeships;
using System;


namespace Dfc.CourseDirectory.Models.Models.Apprenticeships
{
    public class Location : ILocation
    {
        public Guid id { get; set; } // Cosmos DB id

        public Address Address { get; set; }

        public int? ID { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Website { get; set; }

        public string Phone { get; set; }

        public LocationType LocationType { get; set; }
        public int ProviderUKPRN { get; set; } // As we are trying to inforce unique UKPRN per Provider

        public int LocationId { get; set; }
        public int ProviderId { get; set; }

        // Standard auditing properties 
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
