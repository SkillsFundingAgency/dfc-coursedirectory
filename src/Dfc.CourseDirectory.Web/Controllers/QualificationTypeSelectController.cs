using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class QualificationTypeSelectController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}