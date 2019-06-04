﻿using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class RegulatedQualificationController : Controller
    {
        [HttpGet]
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult RegulatedTypeSelected(RegulatedViewModel regulatedViewModel)
        {
            if (regulatedViewModel.RegulatedType == RegulatedType.Regulated)
            {
                return RedirectToAction("Index", "Qualifications");
            }
            else
            {
                return RedirectToAction("Index", "UnregulatedCourses");
            }


        }
    }
}