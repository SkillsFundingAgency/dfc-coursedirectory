using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.DeliveryType
{
    public class DeliveryTypeModel
    {
        //[Required(ErrorMessage = "Select Delivery Mode")]
        public CourseDeliveryMode? DeliveryMode { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string SecondHintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
