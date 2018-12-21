using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.EntryRequirements
{
    public class EntryRequirementsModel
    {
        [MaxLength(500, ErrorMessage = "Entry requirements must be 500 characters or less")]
        public string EntryRequirements { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
