namespace Dfc.CourseDirectory.FindACourseApi.ApiModels
{
    public interface IPagedRequest
    {
        int? Limit { get; set; }
        int? Start { get; set; }
    }
}
