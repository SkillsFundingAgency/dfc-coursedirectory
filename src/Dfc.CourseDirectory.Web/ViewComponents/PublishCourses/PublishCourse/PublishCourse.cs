using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Web.ViewModels.PublishCourses;

namespace Dfc.CourseDirectory.Web.ViewComponents.PublishCourses.PublishCourse
{
    public class PublishCourse : ViewComponent
    {
        public IViewComponentResult Invoke(PublishViewModel model)
        {
            return View("~/ViewComponents/PublishCourses/PublishCourse/Default.cshtml", model);
        }
    }
}
