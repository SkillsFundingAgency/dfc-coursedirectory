namespace Dfc.CourseDirectory.FindACourseApi.ApiModels
{
    public class ProviderSearchRequest
    {
        public string Keyword { get; set; }
        public string[] Town { get; set; }
        public string[] Region { get; set; }
        public int? TopResults { get; set; }
    }
}
