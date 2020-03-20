using Dfc.CourseDirectory.WebV2.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.ViewComponents
{
    public class StandardOrFrameworkIdInputsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(StandardOrFramework standardOrFramework)
        {
            return View("~/SharedViews/Components/StandardOrFrameworkIdInputs.cshtml", standardOrFramework);
        }
    }
}
