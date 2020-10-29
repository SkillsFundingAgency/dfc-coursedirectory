using System;

namespace Dfc.CourseDirectory.FindACourseApi.Models.Search.Faoc
{
    public class OnlineCourseSearchCriteria
    {
        public string SubjectKeyword { get; set; }
        public string ProviderName { get; set; }
        public string[] QualificationLevels { get; set; }
        public string Town { get; set; }
        public CourseSearchSortBy? SortBy { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public int? Limit { get; set; }
        public int? Start { get; set; }
    }
}