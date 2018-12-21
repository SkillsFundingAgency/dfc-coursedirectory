
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace Dfc.CourseDirectory.Web.ViewComponents.CourseProviderReference
{
    public class CourseProviderReferenceModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter an ID")]
        [MaxLength(255, ErrorMessage = "ID must be 255 characters or less")]
        [RegularExpression(@"^\S+(?: \S+)*$", ErrorMessage = "ID must not have any leading, trailing or contain multiple consecutive spaces")]
        public string CourseProviderReference { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
