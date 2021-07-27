using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.Duration
{
    public class DurationModel
    {
        public CourseDurationUnit? DurationUnit { get; set; }
        public List<SelectListItem> DurationUnits { get; set; }
      
        public string DurationLength { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
