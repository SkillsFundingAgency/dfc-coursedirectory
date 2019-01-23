using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.UrlInput
{
    public class UrlInputModel
    {
        [MaxLength(255, ErrorMessage = "The maximum length of URL is 255 characters")]
        [RegularExpression(@"([h|H][t|T][t|T][p|P][s|S]?):\/\/[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-zA-Z]{2,4}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)", ErrorMessage = "The format of URL is incorrect")]
        public string Url { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
