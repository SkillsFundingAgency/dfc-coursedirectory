namespace Dfc.CourseDirectory.Services.Interfaces.CourseService
{
    public interface ICourseServiceSettings
    {
        string ApiUrl { get; set; }
        string ApiKey { get; set; }
        int BulkUploadSecondsPerRecord { get; set; }
    }
}
