using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class HelpDeskController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}