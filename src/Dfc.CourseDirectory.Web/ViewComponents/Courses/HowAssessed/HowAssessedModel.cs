using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.HowAssessed
{
    public class HowAssessedModel
    {
        [MaxLength(500, ErrorMessage = "‘How you’ll be assessed' must be 500 characters or less")]
        [RegularExpression(@"[a-zA-Z0-9 /\r\n?|\n\¬\!\£\$\%\^\&\*\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`\" + "\"" + @"\\]+", ErrorMessage = "How you’ll be assessed contains invalid characters")]
        public string HowAssessed { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
