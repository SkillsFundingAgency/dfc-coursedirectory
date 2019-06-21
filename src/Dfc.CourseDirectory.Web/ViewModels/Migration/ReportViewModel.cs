using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;

namespace Dfc.CourseDirectory.Web.ViewModels.Migration
{
    public class ReportViewModel
    {
        public int ErrorsCount { get; set; }
        public int NoLarsCount { get; set; }
        public int PublishedCount { get; set; }
        public int CourseCount { get; set; }

        public ReportViewModel(IEnumerable<Course> courses)
        {
            var courseCountStatusList = new List<RecordStatus>
            {
                RecordStatus.Live,
                RecordStatus.MigrationPending,
                RecordStatus.MigrationReadyToGoLive
            };

            ErrorsCount = courses.SelectMany(c => c.CourseRuns)
                .Count(x => x.RecordStatus == RecordStatus.MigrationPending);

            PublishedCount = courses.Where(x => x.CourseStatus == RecordStatus.Live)
                .SelectMany(c => c.CourseRuns)
                .Count(x => x.RecordStatus == RecordStatus.Live);

            CourseCount = courses.SelectMany(x => x.CourseRuns)
                .Count(x => courseCountStatusList.Contains(x.RecordStatus));
            //Awaiting new service
            NoLarsCount = 0;
        }
    }
}
