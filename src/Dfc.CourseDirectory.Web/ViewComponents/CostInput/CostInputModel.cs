using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.CostInput
{
    public class CostInputModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter a cost")]
        [MaxLength(8, ErrorMessage = "Cost must be 8 characters or less")]
        [RegularExpression(@"^\d+(?:\.\d{3})*\.\d{2}$", ErrorMessage = "Cost must be a valid money value")]
        public string Cost { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
