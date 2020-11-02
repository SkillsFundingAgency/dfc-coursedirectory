namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public class BulkUploadCourseRun
    {
        public string LearnAimRef { get; set; }
        public int TempCourseId { get; set; }
        public CourseRun CourseRun { get; set; }
    }
}
