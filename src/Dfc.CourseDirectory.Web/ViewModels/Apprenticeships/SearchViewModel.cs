using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Web.ViewModels.Apprenticeships
{
    public class SearchViewModel
    {
        [RegularExpression(@"[a-zA-Z0-9 \¬\!\£\$\%\^\&\*\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`\" + "\"" + @"\\]+", ErrorMessage = "Search contains invalid characters")]
        public string Search { get; set; }
    }
}
