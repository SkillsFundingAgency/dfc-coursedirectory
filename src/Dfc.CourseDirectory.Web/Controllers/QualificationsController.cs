using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers
{
    [Authorize("Fe")]
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
        [Authorize]
        public IActionResult Index()
        {
            _session.SetString("Option", "Qualifications");
            return View();
        }
        [Authorize]
        public IActionResult QualificationsList()
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

                IActionResult view = GetCoursesViewModel("", "", "", "", null);
                CoursesViewModel vm = (CoursesViewModel)(((ViewResult)view).Model);

                IEnumerable<CoursesForQualificationAndCountViewModel> coursesForQualifcationsWithCourseRunsCount = vm.Courses.Value?
                    .Select(c => new CoursesForQualificationAndCountViewModel
                    {
                        QualificationType = c.QualType,

                        CourseRunCount = c.Value.SelectMany(d => d.Value.SelectMany(g => g.CourseRuns)).Count(),

                    }).ToList();


                return View(coursesForQualifcationsWithCourseRunsCount);
            }

            return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

        }

        [Authorize]
        public IActionResult LandingOptions()
        {
            return View("../Courses/LandingOptions/Index",new CoursesLandingViewModel());
        }


        [Authorize]
        private IActionResult GetCoursesViewModel(string status, string learnAimRef,
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