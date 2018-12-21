using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.EntryRequirements
{
    public class EntryRequirements : ViewComponent
    {
        public IViewComponentResult Invoke(EntryRequirementsModel model)
        {
            return View("~/ViewComponents/Courses/EntryRequirements/Default.cshtml", model);
        }
    }
}
