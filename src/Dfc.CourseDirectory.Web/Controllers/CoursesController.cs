using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    [Authorize("Fe")]
    public class CoursesController : Controller
    {
        public IActionResult LandingOptions(CoursesLandingViewModel model)
        {
            switch (model.CoursesLandingOptions)
            {
                case CoursesLandingOptions.Add:
                    return RedirectToAction("Index", "RegulatedQualification");
                case CoursesLandingOptions.Upload:
                    return RedirectToAction("Index","BulkUpload");
                case CoursesLandingOptions.View:
                    return RedirectToAction("Index","ProviderCourses");
                default:
                    return RedirectToAction("LandingOptions", "Qualifications");
            }
        }
    }
}
