using Dfc.CourseDirectory.Services.Models;

namespace Dfc.CourseDirectory.Web.ViewModels.BulkUpload
{
    public class PublishCompleteViewModel
    {

        public int NumberOfCoursesPublished { get; set; }

        public PublishMode Mode { get; set; }

        public int BackgroundPublishMinutes { get; set; }
    }
}
