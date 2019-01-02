using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.AddStartDate
{
    public class AddStartDate : ViewComponent
    {
        public IViewComponentResult Invoke(AddStartDateModel model)
        {
            return View("~/ViewComponents/Courses/AddStartDate/Default.cshtml", model);
        }
    }
}
