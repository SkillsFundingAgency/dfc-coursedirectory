
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Dfc.CourseDirectory.Web.ViewComponents.CourseProviderReference;


namespace Dfc.CourseDirectory.Web.ViewComponents.CourseName
{
    public class CourseProviderReference : ViewComponent
    {
        public IViewComponentResult Invoke(CourseProviderReferenceModel model)
        {
            return View("~/ViewComponents/CourseProviderReference/Default.cshtml", model);
        }
    }
}
