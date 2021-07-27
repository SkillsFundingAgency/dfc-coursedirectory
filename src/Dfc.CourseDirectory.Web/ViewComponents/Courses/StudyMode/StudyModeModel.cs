using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.StudyMode
{
    public class StudyModeModel
    {
        public CourseStudyMode? StudyMode { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
    }
}
