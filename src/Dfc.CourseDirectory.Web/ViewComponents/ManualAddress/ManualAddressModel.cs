using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Web.ViewComponents.ManualAddress
{
    public class ManualAddressModel
    {
        public string  Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter address line 1")]
        [MaxLength(100, ErrorMessage = "Address line 1 must be 100 characters or less")]
        [RegularExpression(@"[a-zA-Z0-9\.\-']+(?: [a-zA-Z0-9\.\-']+)*$", ErrorMessage = "Address line 1 must only include letters a to z, numbers, hyphens and spaces")]
        public string AddressLine1 { get; set; }
        public string AddressLine1LabelText { get; set; }
        public string AddressLine1HintText { get; set; }
        public string AddressLine1AriaDescribedBy { get; set; }

        [MaxLength(100, ErrorMessage = "Address line 2 must be 100 characters or less")]
        [RegularExpression(@"[a-zA-Z0-9\.\-']+(?: [a-zA-Z0-9\.\-']+)*$", ErrorMessage = "Address line 2 must only include letters a to z, numbers, hyphens and spaces")]
        public string AddressLine2 { get; set; }
        public string AddressLine2LabelText { get; set; }
        public string AddressLine2HintText { get; set; }
        public string AddressLine2AriaDescribedBy { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter a town or city")]
        [MaxLength(75, ErrorMessage = "Town or city must be 75 characters or less")]
        [RegularExpression("[a-zA-Z\\.\\-']+(?: [a-zA-Z\\.\\-']+)*$", ErrorMessage = "Town or city must only include letters a to z, numbers, hyphens, spaces, full-stops, and or apostrophes")]
        public string TownOrCity { get; set; }
        public string TownOrCityLabelText { get; set; }
        public string TownOrCityHintText { get; set; }
        public string TownOrCityAriaDescribedBy { get; set; }

        [MaxLength(75, ErrorMessage = "County must be 75 characters or less")]
        [RegularExpression("[a-zA-Z\\.\\-']+(?: [a-zA-Z\\.\\-']+)*$", ErrorMessage = "County must only include letters a to z, numbers, hyphens, spaces, full-stops, and or apostrophes")]
        public string County { get; set; }
        public string CountyLabelText { get; set; }
        public string CountyHintText { get; set; }
        public string CountyAriaDescribedBy { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter a postcode")]
        [MaxLength(8, ErrorMessage = "Postcode must be 8 characters or less")]
        [RegularExpression("([abcdefghijklmnoprstuwyzABCDEFGHIJKLMNOPRSTUWYZ][0-9]|[abcdefghjkstuwABCDEFGHJKSTUW][0-9][0-9]|[abcdefghijklmnoprstuwyzABCDEFGHIJKLMNOPRSTUWYZ][abcdefghklmnopqrstuvwxyABCDEFGHKLMNOPQRSTUVWXY][0-9]|[abcdefghijklmnoprstuwyzABCDEFGHIJKLMNOPRSTUWYZ][abcdefghklmnopqrstuvwxyABCDEFGHKLMNOPQRSTUVWXY][0-9][0-9]|[abcdefghijklmnoprstuwyzABCDEFGHIJKLMNOPRSTUWYZ][0-9][abcdefghjkstuwABCDEFGHJKSTUW]|[abcdefghijklmnoprstuwyzABCDEFGHIJKLMNOPRSTUWYZ][abcdefghklmnopqrstuvwxyABCDEFGHKLMNOPQRSTUVWXY][0-9][abehmnprvwxyABEHMNPRVWXY]) ([0-9][abdefghjlnpqrstuwxyzABDEFGHJLNPQRSTUWXYZ][abdefghjlnpqrstuwxyzABDEFGHJLNPQRSTUWXYZ])", ErrorMessage = "Postcode must be a valid format and only include letters a to z, numbers and spaces")]
        public string Postcode { get; set; }
        public string PostcodeLabelText { get; set; }
        public string PostcodeHintText { get; set; }
        public string PostcodeAriaDescribedBy { get; set; }
    }
}