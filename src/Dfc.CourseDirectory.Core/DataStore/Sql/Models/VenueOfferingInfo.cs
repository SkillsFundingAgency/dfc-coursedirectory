using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class VenueOfferingInfo
    {
        public Venue Venue { get; set; }
        public IReadOnlyCollection<(Guid ApprenticeshipId, Guid ApprenticeshipLocationId)> LinkedApprenticeships { get; set; }
        public IReadOnlyCollection<(Guid CourseId, Guid CourseRunId)> LinkedCourses { get; set; }
        public IReadOnlyCollection<(Guid TLevelId, Guid TLevelLocationId)> LinkedTLevels { get; set; }
    }
}
