using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewComponents.YourCourses.Course
{
    public class Course : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(YourCoursesViewModel model)
        {
            return View("~/ViewComponents/YourCourses/Course/Default.cshtml", model);
        }
    }
}
