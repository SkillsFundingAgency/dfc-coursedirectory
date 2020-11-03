using Dfc.CourseDirectory.Services.Enums;

namespace Dfc.CourseDirectory.Web.ViewModels.BulkUpload
{
    public class PublishCompleteViewModel
    {

        public int NumberOfCoursesPublished { get; set; }

        public PublishMode Mode { get; set; }

        public bool BackgroundPublishInProgress { get; set; }

        public int BackgroundPublishMinutes { get; set; }
    }
}
