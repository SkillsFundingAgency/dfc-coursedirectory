using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Web.ViewComponents.AddVenue
{
    public class AddVenueModel
    {
        public string VenueName { get; set; }

        [Required]
        public string PostCode { get; set; }
    }
}