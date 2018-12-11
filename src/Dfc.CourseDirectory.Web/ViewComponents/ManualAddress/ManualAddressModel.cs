using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Web.ViewComponents.ManualAddress
{
    public class ManualAddressModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter a building and street")]
        [MaxLength(150, ErrorMessage = "Building and street must be 150 characters or less")]
        [RegularExpression("[a-zA-Z0-9']+(?: [a-zA-Z\\.\\-]+)*$", ErrorMessage = "Building and street must only include letters a to z, numbers, hyphens and spaces")]
        public string AddressLine1 { get; set; }
        public string AddressLine1LabelText { get; set; }
        public string AddressLine1HintText { get; set; }
        public string AddressLine1AriaDescribedBy { get; set; }

        [MaxLength(150, ErrorMessage = "Building and street (line 2) must be 150 characters or less")]
        [RegularExpression("[a-zA-Z0-9']+(?: [a-zA-Z\\.\\-]+)*$", ErrorMessage = "Building and street (line 2) must only include letters a to z, numbers, hyphens and spaces")]
        public string AddressLine2 { get; set; }
        public string AddressLine2LabelText { get; set; }
        public string AddressLine2HintText { get; set; }
        public string AddressLine2AriaDescribedBy { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter a town or city")]
        [MaxLength(150, ErrorMessage = "Town or city must be 150 characters or less")]
        [RegularExpression("[a-zA-Z\\.\\-']+(?: [a-zA-Z\\.\\-']+)*$", ErrorMessage = "Town or city must only include letters a to z, numbers, hyphens, spaces, full-stops, and or apostrophes")]
        public string TownOrCity { get; set; }
        public string TownOrCityLabelText { get; set; }
        public string TownOrCityHintText { get; set; }
        public string TownOrCityAriaDescribedBy { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter a county")]
        [MaxLength(150, ErrorMessage = "County must be 150 characters or less")]
        [RegularExpression("[a-zA-Z\\.\\-']+(?: [a-zA-Z\\.\\-']+)*$", ErrorMessage = "County must only include letters a to z, numbers, hyphens, spaces, full-stops, and or apostrophes")]
        public string County { get; set; }
        public string CountyLabelText { get; set; }
        public string CountyHintText { get; set; }
        public string CountyAriaDescribedBy { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter a postcode")]
        [MaxLength(8, ErrorMessage = "Postcode must be 8 characters or less")]
        [RegularExpression("([a-zA-Z][0-9]|[a-zA-Z][0-9][0-9]|[a-zA-Z][a-zA-Z][0-9]|[a-zA-Z][a-zA-Z][0-9][0-9]|[a-zA-Z][0-9][a-zA-Z]|[a-zA-Z][a-zA-Z][0-9][a-zA-Z]) ([0-9][abdefghjklmnpqrstuwxyzABDEFGHJLMNPQRSTUWXYZ][abdefghjklmnpqrstuwxyzABDEFGHJLMNPQRSTUWXYZ])", ErrorMessage = "Postcode must be a valid format and only include letters a to z, numbers and spaces")]
        public string Postcode { get; set; }
        public string PostcodeLabelText { get; set; }
        public string PostcodeHintText { get; set; }
        public string PostcodeAriaDescribedBy { get; set; }
    }
}