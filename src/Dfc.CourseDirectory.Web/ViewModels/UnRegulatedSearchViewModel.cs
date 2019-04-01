using System.ComponentModel.DataAnnotations;
using Dfc.CourseDirectory.Models.Enums;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class UnRegulatedSearchViewModel
    {
        [RegularExpression("^[a-zA-Z][0-9]{8}$", ErrorMessage = "Code must start with Z followed by 8 numbers or letters")]
        [Required(ErrorMessage = "Enter a z code to search")]
        public string Search { get; set; }
    }
}
