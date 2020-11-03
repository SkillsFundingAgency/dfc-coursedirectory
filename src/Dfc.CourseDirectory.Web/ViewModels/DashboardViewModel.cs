
using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Models.Courses;


namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class DashboardViewModel
    {
        public string ValidationHeader { get; set; }
        public IEnumerable<string> ValidationMessages { get; set; }
        public int LiveCourseCount { get; set; }
        public int ArchivedCourseCount { get; set; }
        public int PendingCourseCount { get; set; }
        public IEnumerable<Course> RecentlyModifiedCourses { get; set; }
        public string SuccessHeader { get; set; }

        public DateTime MigrationRunDate { get; set; }

        public int MigrationPendingCount { get; set; }

        public DateTimeOffset? FileUploadDate { get; set; }

        public int BulkUploadPendingCount { get; set; }

        public int BulkUploadReadyToGoLiveCount { get; set; }

        public int BulkUploadTotalCount { get; set; }

        public string MigrationOKMessage { get; set; }
        public string MigrationErrorMessage { get; set; }
        public string BulkUploadMessage { get; set; }

        public EnvironmentType EnvironmentType { get; set; }
        public int FileCount { get; set; }
    }
}
