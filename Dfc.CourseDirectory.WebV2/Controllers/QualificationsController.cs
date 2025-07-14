using Dfc.CourseDirectory.WebV2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Controllers
{
    [Authorize("Fe")]
    public class QualificationsController : Controller
    {

        [Authorize]
        public IActionResult Index()
        {
            HttpContext.Session.SetString("Option", "Qualifications");
            return View();
        }

        [Authorize]
        public IActionResult LandingOptions()
        {
            return View("../Courses/LandingOptions/Index",new CoursesLandingViewModel());
        }
    }
}
