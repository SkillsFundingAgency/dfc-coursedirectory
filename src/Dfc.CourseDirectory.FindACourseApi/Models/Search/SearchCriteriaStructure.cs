using System;

namespace Dfc.CourseDirectory.FindACourseApi.Models
{
    public class SearchCriteriaStructure
    {
        public string SubjectKeyword { get; set; }
        public float? Distance { get; set; }
        public string ProviderName { get; set; }
        public string[] QualificationLevels { get; set; }
        public int[] StudyModes { get; set; }
        public int[] AttendancePatterns { get; set; }
        public int[] DeliveryModes { get; set; }
        public string Town { get; set; }
        public string Postcode { get; set; }
        public CourseSearchSortBy? SortBy { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public int? Limit { get; set; }
        public int? Start { get; set; }
    }
}
