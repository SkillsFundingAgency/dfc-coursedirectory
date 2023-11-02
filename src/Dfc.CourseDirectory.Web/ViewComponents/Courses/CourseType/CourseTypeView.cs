using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseType
{
    public class CourseTypeView : ViewComponent
    {
        public IViewComponentResult Invoke(CourseTypeModel model)
        {
            return View("~/ViewComponents/Courses/CourseType/Default.cshtml", model);
        }
    }
}
