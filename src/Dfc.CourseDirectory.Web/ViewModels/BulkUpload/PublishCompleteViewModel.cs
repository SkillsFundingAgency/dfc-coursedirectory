using Dfc.CourseDirectory.Models.Enums;

namespace Dfc.CourseDirectory.Web.ViewModels.BulkUpload
{
    public class PublishCompleteViewModel
    {
        public int NumberOfCoursesPublished { get; set; }

        public PublishMode Mode { get; set; }
    }
}
