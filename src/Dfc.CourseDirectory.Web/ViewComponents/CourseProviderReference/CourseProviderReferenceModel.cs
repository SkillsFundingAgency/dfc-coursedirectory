
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace Dfc.CourseDirectory.Web.ViewComponents.CourseProviderReference
{
    public class CourseProviderReferenceModel
    {
        //[MaxLength(255, ErrorMessage = "The maximum length of 'ID' is 255 characters")]
        //[RegularExpression(@"[a-zA-Z0-9 \¬\!\£\$\%\^\&\*\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`\" + "\"" + @"\\]+", ErrorMessage = "ID contains invalid characters")]
        public string CourseProviderReference { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
