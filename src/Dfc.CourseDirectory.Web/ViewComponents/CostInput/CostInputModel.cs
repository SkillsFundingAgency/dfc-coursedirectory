using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.CostInput
{
    public class CostInputModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter the cost in pounds and pence")]
        [Range(0.01, 999999.99, ErrorMessage = "Maximum value for cost is £999,999.99")]
        //[RegularExpression(@"^\d+(?:\.\d{3})*\.\d{2}$", ErrorMessage = "Maximum value for cost is £999,999.99")]
        public string Cost { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
