using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseDelivery;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseDeliveryType
{
    public class CourseDeliveryTypeModel
    {
        public List<DeliveryTypeModel> DeliveryTypes { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
