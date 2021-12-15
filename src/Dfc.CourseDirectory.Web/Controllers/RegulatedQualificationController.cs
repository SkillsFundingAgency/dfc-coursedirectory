using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class RegulatedQualificationController : Controller
    {
        private readonly IFeatureFlagProvider _featureFlagProvider;

        public RegulatedQualificationController(IFeatureFlagProvider features)
        {
            _featureFlagProvider = features;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult RegulatedTypeSelected(RegulatedViewModel regulatedViewModel)
        {
            if (regulatedViewModel.WhatAreYouAwarding == WhatAreYouAwarding.Yes)
            {
                if (_featureFlagProvider.HaveFeature(FeatureFlags.CoursesChooseQualification))
                {
                    return RedirectToAction("Get", "ChooseQualification");
                }
                else
                {
                    return RedirectToAction("Index", "Qualifications");
                }
            }
            else
            {
                return RedirectToAction("Index", "UnregulatedCourses");
            }


        }
    }
}
