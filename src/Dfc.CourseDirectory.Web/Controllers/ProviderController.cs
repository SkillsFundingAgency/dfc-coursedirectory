using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class ProviderController : Controller
    {
        private readonly ILogger<ProviderController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly ICourseService _courseService;

        public ProviderController(ILogger<ProviderController> logger,
               IHttpContextAccessor contextAccessor, ICourseService courseService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(courseService, nameof(courseService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            //Set this to 0 so that we display the Add Provider logic within the ProviderSearchResult ViewComponent
            _session.SetInt32("ProviderSearch", 0);
        }
        [Authorize(Policy = "ElevatedUserRole")]
        public IActionResult Index()
        {
            _logger.LogMethodEnter();
            _logger.LogMethodExit();
            return View();
        }

        public async Task<IActionResult> Courses(string qualificationType, Guid? courseId,Guid? courseRunId, string NotificationTitle, string NotificationMessage)
        {
            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            ICourseSearchResult result = (!UKPRN.HasValue
                ? null
                : _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
                    .Result.Value);

            String qualificationTitle = string.Empty;
            String courseName = string.Empty;

            if (courseId.HasValue && courseId.Value != Guid.Empty)
            {
                var course = await _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(courseId.Value));
                 qualificationTitle = course.Value.QualificationCourseTitle;

                if (courseRunId.HasValue)
                {
                    var courseRunCourseName = course.Value.CourseRuns.FirstOrDefault(x => x.id == courseRunId.Value)?.CourseName;
                    courseName = courseRunCourseName;
                }
                
            }

            string notificationMessage = string.Empty;

            if (courseRunId.HasValue)
            {
                 notificationMessage = "<a id='courseeditlink' class='govuk-link' href=# data-courseid=" + courseId +
                                          " data-courserunid = " + courseRunId + "> " + NotificationMessage + " " +
                                       courseName + "</a>";
            }
            else
            {
                 notificationMessage = "<a id='courseeditlink' class='govuk-link' href=# data-courseid=" + courseId +"> " + NotificationMessage + " " +
                                          qualificationTitle + "</a>";
            }


            IEnumerable<ProviderCoursesViewModel> providerCourses= result.Value.FirstOrDefault(o => String.Equals(o.QualType, qualificationType, StringComparison.CurrentCultureIgnoreCase))
                ?.Value
                .Select(c => new ProviderCoursesViewModel()
                {
                    NotificationTitle = NotificationTitle,
                    NotificationMessage = notificationMessage,
                    CourseRunId = courseRunId ?? Guid.Empty,
                    //QualificationTitle = qualificationTitle,
                    CourseId = courseId ?? Guid.Empty,
                   // QualificationType = qualificationType,
                    CoursesForLevel = c.Value,
                }).ToList();


            return View("Courses", providerCourses);
        }
    }
}