using Dfc.CourseDirectory.Services.Models.Courses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.DeliveryType
{
    public class DeliveryTypeModel
    {
        //[Required(ErrorMessage = "Select Delivery Mode")]
        public DeliveryMode DeliveryMode { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string SecondHintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
