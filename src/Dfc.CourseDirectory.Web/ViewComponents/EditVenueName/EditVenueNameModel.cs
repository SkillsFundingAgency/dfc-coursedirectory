using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.EditVenueName
{


    public class EditVenueNameModel
    {
        private const string _whiteSpaceError = "Venue Name Cannot Be Blank";
        private const string _lengthError = "Name cannot be longer than 255 characters.";
        private const string _asciiError = "Please remove any leading/trailing spaces, and enter only ASCII characters (characters on keyboard)";

        public string Id { get; set; }

        [DisplayName("Venue Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = _whiteSpaceError)]
        [StringLength(255, ErrorMessage = _lengthError)]
        [RegularExpression(@"^(?:\w+\s?)+[\x00-\x7F]+$", ErrorMessage =_asciiError)]
        public string VenueName { get; set; }

       

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string PostCode { get; set; }
        public string County { get; set; }
    }

}
