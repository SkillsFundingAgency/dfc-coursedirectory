using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.PostcodeLookup
{
    public class PostcodeLookupModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter a postcode")]
        [MaxLength(8, ErrorMessage = "Postcode must be 8 characters or less")]
        [RegularExpression("([abcdefghijklmnoprstuwyzABCDEFGHIJKLMNOPRSTUWYZ][0-9]|[abcdefghijklmnoprstuwyzABCDEFGHIJKLMNOPRSTUWYZ][0-9][0-9]|[abcdefghijklmnoprstuwyzABCDEFGHIJKLMNOPRSTUWYZ][abcdefghklmnopqrstuvwxyABCDEFGHKLMNOPQRSTUVWXY][0-9]|[abcdefghijklmnoprstuwyzABCDEFGHIJKLMNOPRSTUWYZ][abcdefghklmnopqrstuvwxyABCDEFGHKLMNOPQRSTUVWXY][0-9][0-9]|[abcdefghijklmnoprstuwyzABCDEFGHIJKLMNOPRSTUWYZ][0-9][abcdefghklmnopqrstuvwxyABCDEFGHKLMNOPQRSTUVWXY]|[abcdefghijklmnoprstuwyzABCDEFGHIJKLMNOPRSTUWYZ][abcdefghklmnopqrstuvwxyABCDEFGHKLMNOPQRSTUVWXY][0-9][abcdefghjkstuwABCDEFGHJKSTUW]) ([0-9][abdefghjlnpqrstuwxyzABDEFGHJLMNPQRSTUWXYZ][abdefghjlnpqrstuwxyzABDEFGHJLMNPQRSTUWXYZ])", ErrorMessage = "Postcode must be a valid format and only include letters a to z, numbers and spaces")]
        public string Postcode { get; set; }
        public string PostcodeLabelText { get; set; }
        public string PostcodeHintText { get; set; }
        public string PostcodeAriaDescribedBy { get; set; }
        public string ButtonText { get; set; }
        public IEnumerable<SelectListItem> Items { get; set; }
        public bool HasItems => Items != null && Items.Any();
        public string NoneSelectedText => HasItems ? $"{Items.Count()} addresses found" : string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Select an address")]
        public string PostcodeId { get; set; }

        public PostcodeLookupModel()
        {
            Items = new SelectListItem[] { };
        }
    }
}
