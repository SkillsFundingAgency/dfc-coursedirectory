﻿using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProviderDashboardCounts : ISqlQuery<DashboardCounts>
    {
        public Guid ProviderId { get; set; }
        public DateTime Date { get; set; }
    }

    public class DashboardCounts
    {
        public int CourseRunCount { get; set; }
        public int NonLarsCourseCount { get; set; }
        public int TLevelCount { get; set; }
        public int VenueCount { get; set; }
        public int PastStartDateCourseRunCount { get; set; }
        public int UnpublishedVenueCount { get; set; }
        public int UnpublishedCourseCount { get; set; }
        public int UnpublishedNonLarsCourseCount { get; set; }
        public int PastStartDateNonLarsCourseRunCount { get; set; }
    }
}
