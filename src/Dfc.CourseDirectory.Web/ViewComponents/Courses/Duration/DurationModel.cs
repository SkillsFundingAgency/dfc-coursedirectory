using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseDeliveryType;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.Duration
{
    public class DurationModel
    {
        public string Id { get; set; }
        public List<DurationUnitModel> DurationUnits { get; set; }

        [Required(ErrorMessage = "Enter Duration")]
        public string DurationLength { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }

        public string AriaDescribedBy { get; set; }
    }
}
