using System;
using Dfc.CourseDirectory.FindACourseApi.Models;

namespace Dfc.CourseDirectory.FindACourseApi.ApiModels.Faoc
{
    public class OnlineCourseSearchRequest : IPagedRequest
    {
        public string SubjectKeyword { get; set; }
        public string ProviderName { get; set; }
        public string[] QualificationLevels { get; set; }
        public CourseSearchSortBy? SortBy { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public int? Limit { get; set; }
        public int? Start { get; set; }
    }
}
