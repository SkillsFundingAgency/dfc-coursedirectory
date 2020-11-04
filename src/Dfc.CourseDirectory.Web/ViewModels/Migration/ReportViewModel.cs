using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Web.ViewModels.Migration
{
    public class ReportViewModel
    {
        public int ErrorsCount { get; set; }
        public int? NoLarsCount { get; set; }
        public int PublishedCount { get; set; }
        public int? CourseCount { get; set; }

        public ReportViewModel(IEnumerable<Course> courses, CourseMigrationReport report)
        {
            var courseCountStatusList = new List<RecordStatus>
            {
                RecordStatus.Live,
                RecordStatus.MigrationPending,
                RecordStatus.MigrationReadyToGoLive
            };

            ErrorsCount = courses.Where(c => !string.IsNullOrEmpty(c.LearnAimRef) && !string.IsNullOrEmpty(c.QualificationCourseTitle)) // course does not haev Larns No or qualification title
                                    .SelectMany(c => c.CourseRuns)
                                    .Count(x => x.RecordStatus == RecordStatus.MigrationPending || x.RecordStatus == RecordStatus.MigrationReadyToGoLive);

            PublishedCount = courses.SelectMany(c => c.CourseRuns)
                .Count(x => x.RecordStatus == RecordStatus.Live);

            if (report == null)
            {
                CourseCount = null;
                NoLarsCount = null;
            }
            else
            {
                CourseCount = report.PreviousLiveCourseCount;

                NoLarsCount = (report.LarslessCourses.SelectMany(c => c.CourseRuns)).Count();

            }
        }
    }
}
