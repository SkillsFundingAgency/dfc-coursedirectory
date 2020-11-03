using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships
{
    public class DeliveryOptionsList : ViewComponent
    {
        public IViewComponentResult Invoke(DeliveryOptionSummary model)
        {
            return View("~/ViewComponents/Apprenticeships/DeliveryOptionsList/Default.cshtml", model);
        }
    }
}
