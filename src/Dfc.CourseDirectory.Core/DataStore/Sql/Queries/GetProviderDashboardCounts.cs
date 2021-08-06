using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProviderDashboardCounts : ISqlQuery<DashboardCounts>
    {
        public Guid ProviderId { get; set; }
        public DateTime Date { get; set; }
    }

    public class DashboardCounts
    {
        public IReadOnlyDictionary<CourseStatus, int> CourseRunCounts { get; set; }
        public IReadOnlyDictionary<ApprenticeshipStatus, int> ApprenticeshipCounts { get; set; }
        public IReadOnlyDictionary<TLevelStatus, int> TLevelCounts { get; set; }
        public int VenueCount { get; set; }
        public int PastStartDateCourseRunCount { get; set; }
        public int UnpublishedVenueCount { get; set; }
        public int UnpublishedCourseCount { get; set; }
    }
}
