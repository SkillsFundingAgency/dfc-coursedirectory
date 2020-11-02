using Dfc.CourseDirectory.Services.Enums;

namespace Dfc.CourseDirectory.Web.ViewModels.Migration
{
    public class ErrorsViewModel
    {
        public MigrationErrors MigrationErrors { get; set; }
        public int? LiveCourses { get; set; }
        public int? Errors { get; set; }
    }
}
