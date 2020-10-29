namespace Dfc.CourseDirectory.FindACourseApi.ApiModels
{
    public class PostcodeSearchRequest
    {
        public string Keyword { get; set; }
        public int? TopResults { get; set; }
    }
}
