using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.Summary.SummaryList
{
    public class SummaryList : ViewComponent
    {
        public IViewComponentResult Invoke(SummaryListModel model)
        {
            return View("~/ViewComponents/Summary/SummaryList/Default.cshtml", model);
        }
    }
}
