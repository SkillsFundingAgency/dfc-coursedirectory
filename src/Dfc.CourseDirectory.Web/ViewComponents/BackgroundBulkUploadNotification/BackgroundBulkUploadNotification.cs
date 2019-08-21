using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.BackgroundBulkUploadNotification
{
    public class BackgroundBulkUploadNotification : ViewComponent
    {
        public IViewComponentResult Invoke(BackgroundBulkUploadNotificationModel model)
        {
            return View("~/ViewComponents/BackgroundBulkUploadNotification/Default.cshtml", model);
        }
    }
}
