using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.UrlInput
{
    public class UrlInputModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter a url")]
        [MaxLength(255, ErrorMessage = "Url must be 255 characters or less")]
        [RegularExpression(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,4}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)", ErrorMessage = "Url must be a valid url")]
        public string Url { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
