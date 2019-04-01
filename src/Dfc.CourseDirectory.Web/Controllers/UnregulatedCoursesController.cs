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
        public IActionResult Index()
        {


            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult Index(UnRegulatedSearchViewModel model)
        {


            return View();
        }


        [Authorize]
        public IActionResult UnknownZCode()
        {


            return View();
        }

        


 
    }
}