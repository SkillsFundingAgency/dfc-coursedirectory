using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Web.Models.Apprenticeships
{
    public class ApprenticeshipLocation
    {
        public Guid Id { get; set; }
        public Guid? VenueId { get; set; }
        public Venue Venue { get; set; }
        //public Guid? LocationGuidId { get; set; }
        //public int? LocationId { get; set; }
        public bool? National { get; set; }
        //public ApprenticeshipLocationAddress Address { get; set; }
        public List<int> DeliveryModes { get; set; }
        public string Name { get; set; }
        //public string Phone { get; set; }
        //public int ProviderUKPRN { get; set; }
        public IReadOnlyCollection<string> SubRegionIds { get; set; }
        public ApprenticeshipLocationType ApprenticeshipLocationType { get; set; }
        //public LocationType LocationType { get; set; }
        public int? Radius { get; set; }
        //public ApprenticeshipStatus RecordStatus { get; set; }
        //public DateTime CreatedDate { get; set; }
        //public string CreatedBy { get; set; }
        //public DateTime? UpdatedDate { get; set; }
        //public string UpdatedBy { get; set; }

        public CreateApprenticeshipLocation CreateCommand() => throw new NotImplementedException();
    }
}
