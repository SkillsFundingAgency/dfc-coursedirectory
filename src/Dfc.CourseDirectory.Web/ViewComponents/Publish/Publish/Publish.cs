using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Web.ViewModels.Publish;

namespace Dfc.CourseDirectory.Web.ViewComponents.Publish.Publish
{
    public class Publish : ViewComponent
    {
        public IViewComponentResult Invoke(PublishViewModel model)
        {
            return View("~/ViewComponents/Publish/Publish/Default.cshtml", model);
        }
    }
}
