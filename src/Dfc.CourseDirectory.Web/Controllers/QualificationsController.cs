using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class QualificationsController : Controller
    {
        private readonly ILogger<QualificationsController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly ICourseService _courseService;


        public QualificationsController(
            ILogger<QualificationsController> logger,
            IHttpContextAccessor contextAccessor, ICourseService courseService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult QualificationsList()
        {
            var qualificationTypes = new List<string>();
            var UKPRN = _session.GetInt32("UKPRN");

            if (UKPRN.HasValue)
            {
                var coursesByUKPRN = !UKPRN.HasValue
                    ? null
                    : _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
                        .Result.Value;

                foreach (var a in coursesByUKPRN.Value)
                {
                    foreach (var b in a.Value)
                    {
                        foreach (var c in b.Value)
                        {
                            qualificationTypes.Add(c.QualificationType);
                        }
                    }
                }

                qualificationTypes = qualificationTypes.Distinct().ToList();
            }


            return View(qualificationTypes);
        }

        public async Task<IActionResult> Courses(string qualificationType)
        {
            var x =HttpUtility.UrlDecode(qualificationType);
            IActionResult view = await GetCoursesViewModelAsync("", "", "", "", null);
            CoursesViewModel vm = (CoursesViewModel)(((ViewResult)view).Model);
           // IEnumerable<ICourseSearchInnerGrouping> coursesForQualifcation = vm.Courses.Value.FirstOrDefault(o => o.QualType == qualificationType).Value.Select(i => new CourseSearchInnerResultGrouping(i.LARSRef));

            //var courses = vm.Courses.Value.FirstOrDefault(o => o.QualType == qualificationType)
            //    .Value
            //    .Select(t => new CourseSearchInnerGrouping { Course = t.Value});


            //IEnumerable<Course> courses = vm.Courses.Value.FirstOrDefault(o => o.QualType == qualificationType)
            //    .Value
            //    .Select(c => c.WithNoCourseRuns());

            return View("Courses");
        }


        private async Task<IActionResult> GetCoursesViewModelAsync(string status, string learnAimRef,
            string numberOfNewCourses, string errmsg, Guid? updatedCourseId)
        {
            if (!string.IsNullOrEmpty(status))
            {
                ViewData["Status"] = status;
                switch (status.ToUpper())
                {
                    case "GOOD":
                        ViewData["StatusMessage"] =
                            string.Format("{0} New Course(s) created in Course Directory for LARS: {1}",
                                numberOfNewCourses, learnAimRef);
                        break;
                    case "BAD":
                        ViewData["StatusMessage"] = errmsg;
                        break;
                    case "UPDATE":
                        ViewData["StatusMessage"] = string.Format("Course run updated in Course Directory");
                        break;
                    default:
                        break;
                }
            }

            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            ICourseSearchResult result = (!UKPRN.HasValue
                ? null
                : _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
                    .Result.Value);


            CoursesViewModel vm = new CoursesViewModel
            {
                UKPRN = UKPRN,
                Courses = result,
            };

            return View(vm);
        }


    }
}