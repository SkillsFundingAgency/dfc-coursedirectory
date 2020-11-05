using System;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class HomeController : Controller
    {
        [AllowDeactivatedProvider]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowDeactivatedProvider]
        public IActionResult Help()
        {
            return View();
        }
    }
}
