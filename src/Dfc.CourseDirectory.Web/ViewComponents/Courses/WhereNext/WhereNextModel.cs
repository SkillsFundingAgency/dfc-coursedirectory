using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.WhereNext
{
    public class WhereNextModel
    {
        [MaxLength(500, ErrorMessage = "'Where next' must be 500 characters or less")]
        public string WhereNext { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
