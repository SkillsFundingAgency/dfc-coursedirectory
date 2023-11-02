using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseType
{
    public class CourseTypeModel
    {
        //[Required(ErrorMessage = "Select Delivery Mode")]
        public CourseType? CourseType { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string SecondHintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
