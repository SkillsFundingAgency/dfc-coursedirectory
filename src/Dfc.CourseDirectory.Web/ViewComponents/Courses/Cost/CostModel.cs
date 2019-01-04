using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.Cost
{
    public class CostModel
    {

        public string Cost { get; set; }
        public string CostLabelText { get; set; }
        public string CostHintText { get; set; }
        public string CostAriaDescribedBy { get; set; }


        [MaxLength(255, ErrorMessage = "Cost description must be 255 characters or less")]
        [RegularExpression(@"[a-zA-Z0-9 \¬\!\£\$\%\^\&\*\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`\" + "\"" + @"\\]+", ErrorMessage = "Cost Description contains invalid characters")]
        public string CostDescription { get; set; }
        public string CostDescriptionLabelText { get; set; }
        public string CostDescriptionHintText { get; set; }
        public string CostDescriptionAriaDescribedBy { get; set; }

    }
}
