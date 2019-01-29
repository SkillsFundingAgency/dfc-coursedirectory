using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Notification
{
    public class Notification : ViewComponent
    {
        public IViewComponentResult Invoke(NotificationModel model)
        {
            return View("~/ViewComponents/Notification/Default.cshtml", model);
        }
    }
}
