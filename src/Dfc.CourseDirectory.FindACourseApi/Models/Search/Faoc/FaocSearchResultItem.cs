namespace Dfc.CourseDirectory.FindACourseApi.Models.Search.Faoc
{
    public class FaocSearchResultItem
    {
        public AzureSearchOnlineCourse Course { get; set; }
        public double Score { get; set; }
    }
}