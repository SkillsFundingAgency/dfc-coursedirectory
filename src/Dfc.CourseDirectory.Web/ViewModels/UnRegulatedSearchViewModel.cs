using System.ComponentModel.DataAnnotations;
using Dfc.CourseDirectory.Models.Enums;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class UnRegulatedSearchViewModel
    {
        [RegularExpression("^[zZ]{1}[a-zA-Z0-9]{7}$", ErrorMessage = "Code must start with Z followed by 7 numbers or letters")]
        [Required(ErrorMessage = "Enter a z code to search")]
        public string Search { get; set; }

        public string NotificationTitle { get; set; }

        public string NotificationMessage { get; set; }
    }
}
