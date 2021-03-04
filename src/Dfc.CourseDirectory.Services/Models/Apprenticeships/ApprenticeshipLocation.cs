using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Services.Models.Apprenticeships
{
    public class ApprenticeshipLocation
    {
        public Guid Id { get; set; }
        public Guid? VenueId { get; set; }
        public Guid? LocationGuidId { get; set; }
        public int? LocationId { get; set; }
        public bool? National { get; set; }
        public Address Address { get; set; }
        public List<int> DeliveryModes { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public int ProviderUKPRN { get; set; }
        public IEnumerable<string> Regions { get; set; }
        public ApprenticeshipLocationType ApprenticeshipLocationType { get; set; }
        public LocationType LocationType { get; set; }
        public int? Radius { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
