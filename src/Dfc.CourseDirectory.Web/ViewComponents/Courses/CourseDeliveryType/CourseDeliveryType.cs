using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseDeliveryType;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseDelivery
{
    public class CourseDeliveryType : ViewComponent
    {
        public IViewComponentResult Invoke(CourseDeliveryTypeModel model)
        {
            return View("~/ViewComponents/Courses/CourseDeliveryType/Default.cshtml", model);
        }
    }
}
