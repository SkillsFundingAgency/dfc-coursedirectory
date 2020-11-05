using Dfc.CourseDirectory.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class DQIController : Controller
    {
        private ISession Session => HttpContext.Session;

        [Authorize]
        public IActionResult Index(string msg)
        {
            Session.SetString("Option", "DQI");
            return RedirectToAction("Index", "PublishCourses", new { publishMode = PublishMode.DataQualityIndicator });
        }
    }
}
