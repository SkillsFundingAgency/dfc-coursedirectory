using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dfc.CourseDirectory.Models.Models.Courses;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.Duration
{
    public class DurationModel
    {
        public DurationUnit DurationUnit { get; set; }
        // public List<DurationUnitModel> DurationUnits { get; set; }
        public List<SelectListItem> DurationUnits { get; set; }
        [Required(ErrorMessage = "Enter Duration")]
        public string DurationLength { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
