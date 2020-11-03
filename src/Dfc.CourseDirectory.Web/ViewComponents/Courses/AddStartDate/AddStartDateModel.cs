using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.AddStartDate
{
    public class AddStartDateModel
    {
        public string LabelText { get; set; }
        public string SpecifiedDateHintText { get; set; }
        public string FlexibleDateHintText { get; set; }
        public string AriaDescribedBy { get; set; }

        public StartDateType StartDateType { get; set; }


        public string Day { get; set; }
        public string DayAriaDescribedBy { get; set; }
        public string DayLabelText { get; set; }


        public string Month { get; set; }
        public string MonthAriaDescribedBy { get; set; }
        public string MonthLabelText { get; set; }


        public string Year { get; set; }
        public string YearAriaDescribedBy { get; set; }
        public string YearLabelText { get; set; }

        public DateTime? CurrentStartDate { get; set; }

        public DateTime ValPastDateRef { get; set; }
        public string ValPastDateMessage { get; set; }
    }
}
