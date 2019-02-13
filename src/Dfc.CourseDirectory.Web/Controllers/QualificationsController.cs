using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Internal;
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
        private readonly IVenueService _venueService;


        public QualificationsController(
            ILogger<QualificationsController> logger,
            IHttpContextAccessor contextAccessor, ICourseService courseService, IVenueService venueService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(venueService, nameof(venueService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            _venueService = venueService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> QualificationsList()
        {
            var qualificationTypes = new List<string>();

            var providerUKPRN = User.Claims.SingleOrDefault(x => x.Type == "UKPRN");
            if (providerUKPRN != null)
            {
                _session.SetInt32("UKPRN", Int32.Parse(providerUKPRN.Value));
            }

            var UKPRN = _session.GetInt32("UKPRN");



            List<QualificationViewModel> qualificationsList = new List<QualificationViewModel>();

            if (UKPRN.HasValue)
            {
                QualificationViewModel qualification = new QualificationViewModel();

                var coursesByUKPRN = !UKPRN.HasValue
                    ? null
                    : _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
                        .Result.Value;

                IActionResult view = await GetCoursesViewModelAsync("", "", "", "", null);
                CoursesViewModel vm = (CoursesViewModel)(((ViewResult)view).Model);

                IEnumerable<CoursesForQualificationAndCountViewModel> coursesForQualifcationsWithCourseRunsCount = vm.Courses.Value?
                    .Select(c => new CoursesForQualificationAndCountViewModel
                    {
                        QualificationType = c.QualType,
                        //Course = c.Value.FirstOrDefault(),
                       // Courses = c.Value,
                        CourseRunCount = c.Value.SelectMany(d => d.Value.SelectMany(g=>g.CourseRuns)).Count(),
                        //CourseRuns = c.Value.FirstOrDefault()?.CourseRuns
                        //CourseRuns =  c.Value.SelectMany(d => d.CourseRuns)
                    }).ToList();

                //var courseRunTotal = 0;
                //foreach (var a in coursesByUKPRN.Value)
                //{
                //    foreach (var b in a.Value)
                //    {
                //        foreach (var c in b.Value)
                //        {
                //            courseRunTotal += c.CourseRuns.Count();
                //            qualificationTypes.Add(c.QualificationType);

                //            qualification.CourseRunCount = courseRunTotal.ToString();
                //            qualification.QualificationTitle = c.QualificationType;

                //            qualificationsList.Add(qualification);
                //        }
                //    }
                //}

                //qualificationTypes = qualificationTypes.Distinct().ToList();

                return View(coursesForQualifcationsWithCourseRunsCount);
            }

            return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

        }

        public async Task<IActionResult> Courses(string qualificationType, Guid? courseId, Guid? courseRunId, CourseMode courseMode)
        {
            IActionResult view = await GetCoursesViewModelAsync("", "", "", "", null);
            CoursesViewModel vm = (CoursesViewModel)(((ViewResult)view).Model);

            if (courseId.HasValue)
            {
                ViewBag.CourseId = courseId.Value;
            }

            if (courseRunId.HasValue)
            {
                ViewBag.CourseRunId = courseRunId.Value;
            }

            ViewBag.CourseMode = courseMode;


            IEnumerable<CoursesForQualificationAndCountViewModel> coursesForQualifcationsWithCourseRunsCount = vm.Courses.Value.FirstOrDefault(o => String.Equals(o.QualType, qualificationType, StringComparison.CurrentCultureIgnoreCase))
               ?.Value
                .Select(c => new CoursesForQualificationAndCountViewModel
                {
                    QualificationType = qualificationType,
                    //Course = c.Value.FirstOrDefault(),
                    Courses = c.Value,
                    //CourseRunCount = c.Value.SelectMany(d => d.CourseRuns).Count(),
                    //CourseRuns = c.Value.FirstOrDefault()?.CourseRuns
                    //CourseRuns =  c.Value.SelectMany(d => d.CourseRuns)
                }).ToList();


            return View("Courses", coursesForQualifcationsWithCourseRunsCount);
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