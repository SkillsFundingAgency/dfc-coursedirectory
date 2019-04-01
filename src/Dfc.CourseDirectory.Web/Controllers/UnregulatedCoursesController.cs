using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class UnregulatedCoursesController : Controller
    {
        [Authorize]
        public IActionResult Index(string NotificationTitle, string NotificationMessage)
        {
            var model = new UnRegulatedSearchViewModel()
                    {NotificationTitle = NotificationTitle, NotificationMessage = NotificationMessage};
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Index(UnRegulatedSearchViewModel model)
        {
            if (model.Search.ToLower() == "z9999999")
            {
                return RedirectToAction("Index", "UnregulatedCourses",
                    new
                    {
                        NotificationTitle = "Z code does not exist",
                        NotificationMessage = "Check the code you have entered and try again"
                    });
            }

            return View("ZCodeResults");
        }


        [Authorize]
        public IActionResult UnknownZCode()
        {


            return View();
        }

        


 
    }
}