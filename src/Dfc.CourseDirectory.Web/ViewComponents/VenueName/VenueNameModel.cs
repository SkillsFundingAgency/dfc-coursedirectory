using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.VenueName
{
    public class VenueNameModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter a venue name")]
        [MaxLength(255, ErrorMessage = "Venue name must be 255 characters or less")]
        [RegularExpression(@"^\S+(?: \S+)*$", ErrorMessage = "Venue name must not have any leading, trailing or contain multiple consecutive spaces")]
        public string VenueName { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
