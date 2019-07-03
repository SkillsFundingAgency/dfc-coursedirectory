using System;
using Dfc.CourseDirectory.Models.Models.Courses;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewModels.Migration
{
    public class LarslessViewModel
    {
        public IEnumerable<Course> LarslessCourses { get; set; }
        public Dictionary<Guid,string> Venues { get; set; }
    }
}
