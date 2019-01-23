using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.Duration
{
    public class DurationModel
    {
        public int Id { get; set; }
        public List<DurationUnitModel> DurationUnits { get; set; }

        [Required(ErrorMessage = "Enter Duration")]
        public string DurationLength { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
