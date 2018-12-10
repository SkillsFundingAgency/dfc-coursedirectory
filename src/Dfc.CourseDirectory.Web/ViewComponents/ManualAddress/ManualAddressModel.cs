using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Web.ViewComponents.ManualAddress
{
    public class ManualAddressModel
    {
        public string Id { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Required(ErrorMessage = "Venue name is required", AllowEmptyStrings = false)]
        [StringLength(255, ErrorMessage = "The maximum length of Venue Name is 255 characters")]
        [RegularExpression(@"^[\x00-\x7F]+$", ErrorMessage = "Only english based characters allowed")]
        public string VenueName { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Required(ErrorMessage = "Address Line 1 is required", AllowEmptyStrings = false)]
        [StringLength(100, ErrorMessage = "The maximum length of Address Line 1 is 100 characters")]
        [RegularExpression(@"^[\x00-\x7F]+$")]
        public string AddressLine1 { get; set; }

        [StringLength(100, ErrorMessage = "The maximum length of Address Line 2 is 100 characters")]
        [RegularExpression(@"^[\x00-\x7F]+$")]
        public string AddressLine2 { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Required(ErrorMessage = "Town or City is required", AllowEmptyStrings = false)]
        [StringLength(75, ErrorMessage = "The maximum length of Town or City is 75 characters")]
        [RegularExpression(@"^[\x00-\x7F]+$")]
        public string TownCity { get; set; }

        [StringLength(75, ErrorMessage = "The maximum length of County is 75 characters")]
        [RegularExpression(@"^[\x00-\x7F]+$")]
        public string County { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Required(ErrorMessage = "Postcode is required", AllowEmptyStrings = false)]
        [RegularExpression(@"([a-zA-Z][0-9]|[a-zA-Z][0-9][0-9]|[a-zA-Z][a-zA-Z][0-9]|[a-zA-Z][a-zA-Z][0-9][0-9]|[a-zA-Z][0-9][a-zA-Z]|[a-zA-Z][a-zA-Z][0-9][a-zA-Z]) ([0-9][abdefghjklmnpqrstuwxyzABDEFGHJLMNPQRSTUWXYZ][abdefghjklmnpqrstuwxyzABDEFGHJLMNPQRSTUWXYZ])", ErrorMessage = "Enter a valid postcode")]
        public string Postcode { get; set; }
    }
}
