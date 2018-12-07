using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Web.ViewComponents.ManualAddress
{
    public class ManualAddressModel
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Venue name is required") ]
        public string VenueName { get; set; }

        [Required(ErrorMessage = "Building/Street is required")]
        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        [Required(ErrorMessage = "Town or City is required")]
        public string TownCity { get; set; }

        public string County { get; set; }

        [Required(ErrorMessage = "Postcode is required")]
        public string Postcode { get; set; }
    }
}
