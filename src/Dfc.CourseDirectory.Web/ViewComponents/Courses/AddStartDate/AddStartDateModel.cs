using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Courses;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.AddStartDate
{
    public class AddStartDateModel
    {
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Enter who is the course for")]
        //[MaxLength(500, ErrorMessage = "Who is this course for must be 500 characters or less")]
        //public string CourseFor { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
        public StartDateType StartDateType { get; set; }

        [Range(0, 31, ErrorMessage = "Day must be a number between 1 and 31")]
        public string Day { get; set; }
        public string DayAriaDescribedBy { get; set; }
        public string DayLabelText { get; set; }

        [Range(0, 12, ErrorMessage = "Month must be a number between 1 and 12")]
        public string Month { get; set; }
        public string MonthAriaDescribedBy { get; set; }
        public string MonthLabelText { get; set; }

        [MaxLength(4, ErrorMessage = "Year must be a valid 4 digit year")]
        [MinLength(4, ErrorMessage = "Year must be a valid 4 digit year")]
        public string Year { get; set; }
        public string YearAriaDescribedBy { get; set; }
        public string YearLabelText { get; set; }

        public DateTime StartDate { get; set; }

       
    }
}
