
using Dfc.CourseDirectory.Services.Models.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;


namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class CoursesViewModel
    {
        public int? UKPRN { get; set; }

        public ICourseSearchResult Courses { get; set; }
      
    }
}
