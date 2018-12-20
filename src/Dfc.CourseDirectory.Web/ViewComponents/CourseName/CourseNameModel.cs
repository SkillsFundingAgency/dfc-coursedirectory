using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.CourseName
{
    public class CourseNameModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter a course name")]
        [MaxLength(255, ErrorMessage = "Course name must be 255 characters or less")]
        [RegularExpression(@"^\S+(?: \S+)*$", ErrorMessage = "Course name must not have any leading, trailing or contain multiple consecutive spaces")]
        public string CourseName { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
