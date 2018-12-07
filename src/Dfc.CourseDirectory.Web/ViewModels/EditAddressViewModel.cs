using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class EditAddressViewModel
    {

        [Required(AllowEmptyStrings = false, ErrorMessage = "Venue Name Cannot Be Blank")]
        [RegularExpression(@"^[\x00-\x7F]+$")]
        public string VenueName { get; set; }
    }
}
