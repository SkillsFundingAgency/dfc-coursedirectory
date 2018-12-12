using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.ProviderSearch
{
    public class ProviderSearchModel
    {
        [RegularExpression("^[1][0-9]{7}$", ErrorMessage = "UKPRN is 8 digit number starting with a 1 e.g. 10000364")]
        [Required(ErrorMessage = "UKPRN required")]
        public string SearchTerm { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
