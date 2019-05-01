﻿using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interface.Apprenticeships;
using System;


namespace Dfc.CourseDirectory.Models.Models.Apprenticeships
{
    public class Location : ILocation
    {
        public Guid id { get; set; } // Cosmos DB id

        public LocationType LocationType { get; set; }
        public int ProviderUKPRN { get; set; } // As we are trying to inforce unique UKPRN per Provider

        public int LocationId { get; set; }
        public int ProviderId { get; set; }
        public string LocationName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }

        // Standard auditing properties 
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
