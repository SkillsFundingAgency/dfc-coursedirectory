using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.AddStartDate
{
    public class AddStartDateModel
    {
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Enter who is the course for")]
        //[MaxLength(500, ErrorMessage = "Who is this course for must be 500 characters or less")]
        //public string CourseFor { get; set; }
        //public string LabelText { get; set; }
        //public string HintText { get; set; }
        //public string AriaDescribedBy { get; set; }

        public string Day { get; set; }
        public string Month { get; set; }

        public string Year { get; set; }
    }
}
