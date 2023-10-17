using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.WebV2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers
{
    [Authorize("Fe")]
    public class CoursesController : Controller
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly IFeatureFlagProvider _featureFlagProvider;
        private readonly ILogger<CoursesController> _log;

        public CoursesController(IFeatureFlagProvider features, IProviderContextProvider providerContextProvider, ILogger<CoursesController> log)
        {
            _featureFlagProvider = features;
            _providerContextProvider = providerContextProvider;
            _log = log;
        }

        public IActionResult LandingOptions(CoursesLandingViewModel model)
        {
            switch (model.CoursesLandingOptions)
            {
                case CoursesLandingOptions.Add:
                    {
                        _log.LogInformation($"Course controllder Adding Landingoptions ");
                        if (_featureFlagProvider.HaveFeature(FeatureFlags.CoursesChooseQualification))
                        {
                            return RedirectToAction("ChooseQualification", "ChooseQualification").WithProviderContext(_providerContextProvider.GetProviderContext(withLegacyFallback: true)); ;
                        }
                        else
                        {
                            return RedirectToAction("Index", "RegulatedQualification");
                        }
                    }

                case CoursesLandingOptions.Upload:
                    return RedirectToAction("Index", "CoursesDataManagement")
                        .WithProviderContext(_providerContextProvider.GetProviderContext(withLegacyFallback: true));
                case CoursesLandingOptions.View:
                    return RedirectToAction("Index","ProviderCourses");
                default:
                    return RedirectToAction("LandingOptions", "Qualifications");
            }
        }
    }
}
