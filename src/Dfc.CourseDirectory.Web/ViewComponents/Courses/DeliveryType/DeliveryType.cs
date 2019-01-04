using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.DeliveryType
{
    public class DeliveryType : ViewComponent
    {
        public IViewComponentResult Invoke(DeliveryTypeModel model)
        {
            return View("~/ViewComponents/Courses/DeliveryType/Default.cshtml", model);
        }
    }
}
