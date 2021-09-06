using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class ApprenticeshipLocation
    {
        public Guid ApprenticeshipLocationId { get; set; }
        public ApprenticeshipLocationType ApprenticeshipLocationType { get; set; }
        public Venue Venue { get; set; }
        public bool? National { get; set; }
        public IReadOnlyCollection<string> SubRegionIds { get; set; }
        public int? Radius { get; set; }
        public IReadOnlyCollection<ApprenticeshipDeliveryMode> DeliveryModes { get; set; }
        public string Telephone { get; set; }
    }
}
