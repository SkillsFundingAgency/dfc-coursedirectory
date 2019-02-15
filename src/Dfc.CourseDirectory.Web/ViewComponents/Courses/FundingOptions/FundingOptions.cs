using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.FundingOptions
{
    public class FundingOptions : ViewComponent
    {
        public IViewComponentResult Invoke(FundingOptionsModel model)
        {
            return View("~/ViewComponents/Courses/FundingOptions/Default.cshtml", model);
        }
    }
}
