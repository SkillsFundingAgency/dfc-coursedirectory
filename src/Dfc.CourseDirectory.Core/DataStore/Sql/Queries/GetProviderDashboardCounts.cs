using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProviderDashboardCounts : ISqlQuery<(IReadOnlyDictionary<CourseStatus, int> CourseRunCounts, int ApprenticeshipCount, int VenueCount, int pastStartDateCourseRunCount)>
    {
        public Guid ProviderId { get; set; }

        public DateTimeOffset Date { get; set; }
    }
}
