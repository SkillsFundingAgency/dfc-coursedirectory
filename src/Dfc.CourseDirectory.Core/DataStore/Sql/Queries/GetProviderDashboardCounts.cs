using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProviderDashboardCounts : ISqlQuery<(IReadOnlyDictionary<CourseStatus, int> CourseRunCounts, IReadOnlyDictionary<ApprenticeshipStatus, int> ApprenticeshipCounts, int VenueCount, int PastStartDateCourseRunCount, int BulkUploadCoursesErrorCount, int BulkUploadCourseRunsErrorCount, int ApprenticeshipsBulkUploadErrorCount)>
    {
        public Guid ProviderId { get; set; }

        public DateTimeOffset Date { get; set; }
    }
}
