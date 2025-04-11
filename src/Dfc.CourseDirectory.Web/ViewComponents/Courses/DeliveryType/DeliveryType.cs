using Microsoft.AspNetCore.Mvc;

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
