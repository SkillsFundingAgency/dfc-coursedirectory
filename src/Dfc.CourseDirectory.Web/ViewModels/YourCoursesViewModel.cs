using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class YourCoursesViewModel
    {
        public int? UKPRN { get; set; }
        public IEnumerable<Course> Courses { get; set; }
    }
}
