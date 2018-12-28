using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewModels;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class AddCourseRequestModel
    {
        public string CourseName { get; set; }

        public AddCourseViewModel CourseInfo { get; set; }
    }
}
