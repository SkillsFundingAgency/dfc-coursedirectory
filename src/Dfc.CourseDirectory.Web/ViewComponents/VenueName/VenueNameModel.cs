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
        [RegularExpression("[a-zA-Z0-9\\.\\-']+(?: [a-zA-Z0-9\\.\\-']+)*$", ErrorMessage = "Venue name must only include letters a to z, numbers, hyphens, spaces, full-stops, and or apostrophes")]
        public string VenueName { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
