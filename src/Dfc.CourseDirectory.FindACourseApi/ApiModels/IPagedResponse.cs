namespace Dfc.CourseDirectory.FindACourseApi.ApiModels
{
    interface IPagedResponse
    {
        int Limit { get; set; }
        int Start { get; set; }
        int Total { get; set; }
    }
}
