using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.CourseName
{
    public class CourseNameModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter Course Name")]
        [MaxLength(255, ErrorMessage = "The maximum length of Course Name is 255 characters")]
        [RegularExpression(@"[a-zA-Z0-9 \¬\!\£\$\%\^\&\*\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`\" + "\"" + "]+", ErrorMessage = "Invalid characters")]
        public string CourseName { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
