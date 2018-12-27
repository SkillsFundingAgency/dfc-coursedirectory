using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.StudyMode
{
    public class StudyMode : ViewComponent
    {
        public IViewComponentResult Invoke(StudyModeModel model)
        {
            return View("~/ViewComponents/Courses/StudyMode/Default.cshtml", model);
        }
    }
}
