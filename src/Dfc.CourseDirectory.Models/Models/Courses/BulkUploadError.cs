namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public class BulkUploadError
    {
        public int LineNumber { get; set; }
        public string Header { get; set; }
        public string Error { get; set; }
    }
}
