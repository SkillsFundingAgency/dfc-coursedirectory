using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.WebV2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class RegulatedQualificationController : Controller
    {
        private readonly IFeatureFlagProvider _featureFlagProvider;
        private readonly IProviderContextProvider _providerContext;

        public RegulatedQualificationController(IFeatureFlagProvider features, IProviderContextProvider providerContextProvider)
        {
            _featureFlagProvider = features;
            _providerContext = providerContextProvider;
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
                return RedirectToAction("Index", "Qualifications");
            }
            else
            {
                return RedirectToAction("Index", "UnregulatedCourses");
            }
        }
    }
}
