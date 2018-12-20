using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.RequestModels;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class CoursesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddCourseSection1()
        {
            return View();
        }

        public IActionResult AddCourseSection2(AddCourseRequestModel requestModel)
        {
            return View();
        }
    }
}