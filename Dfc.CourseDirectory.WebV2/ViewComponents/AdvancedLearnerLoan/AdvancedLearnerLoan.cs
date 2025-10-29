using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.AdvancedLearnerLoan
{
    public class AdvancedLearnerLoan : ViewComponent 
    {
        public IViewComponentResult Invoke(AdvancedLearnerLoanModel model)
        {
            return View("~/ViewComponents/AdvancedLearnerLoan/Default.cshtml", model);
        }
    }
}
