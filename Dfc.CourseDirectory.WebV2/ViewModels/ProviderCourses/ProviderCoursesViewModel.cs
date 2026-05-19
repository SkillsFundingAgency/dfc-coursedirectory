using System.Collections.Generic;
using Dfc.CourseDirectory.WebV2.ViewComponents.GdsPagination;

namespace Dfc.CourseDirectory.WebV2.ViewModels.ProviderCourses
{
    public class ProviderCoursesViewModel
    {
        public string Keyword { get; set; }
        public bool HasFilters { get; set; }
        public IList<ProviderCourseRunViewModel> ProviderCourseRuns { get; set; }
        public List<ProviderCoursesFilterItemModel> Levels { get; set; }
        public List<ProviderCoursesFilterItemModel> DeliveryModes { get; set; }
        public List<ProviderCoursesFilterItemModel> Venues { get; set; }
        public List<ProviderCoursesFilterItemModel> Regions { get; set; }
        public List<ProviderCoursesFilterItemModel> AttendancePattern { get; set; }
        public int? PendingCoursesCount { get; set; }
        public int TotalCoursesCount { get; set; }
        public string NotificationTitle { get; set; }
        public string NotificationMessage { get; set; }
        public string CourseRunId { get; set; }
        public bool NonLarsCourse { get; set; }
        public GdsPaginationModel Pagination { get; set; }
        public string Nonce { get; set; }
    }
}
