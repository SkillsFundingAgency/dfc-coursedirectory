using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.Notification
{
    public class Notification : ViewComponent
    {
        public IViewComponentResult Invoke(NotificationModel model)
        {
            return View("~/ViewComponents/Notification/Default.cshtml", model);
        }
    }
}
