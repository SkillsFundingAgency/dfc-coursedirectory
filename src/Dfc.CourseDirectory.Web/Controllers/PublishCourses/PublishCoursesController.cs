using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.ViewModels.PublishCourses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers.PublishCourses
{
    public class PublishCoursesController : Controller
    {
        private ISession Session => HttpContext.Session;
        private readonly ICourseService _courseService;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public PublishCoursesController(
            ICourseService courseService,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(PublishMode publishMode, string notificationTitle, Guid? courseId, Guid? courseRunId, bool fromBulkUpload)
        {
            int? UKPRN = Session.GetInt32("UKPRN");
            
            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var coursesByUKPRN = (await _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))).Value;

            // Only display courses that have Lars and Qualification titles
            var courses = coursesByUKPRN.Value
                .SelectMany(o => o.Value)
                .SelectMany(i => i.Value)
                .Where(c => !string.IsNullOrWhiteSpace(c.LearnAimRef) && !string.IsNullOrWhiteSpace(c.QualificationCourseTitle))
                .ToList();

            courses = GetErrorMessages(courses, ValidationMode.MigrateCourse).ToList();

            PublishViewModel vm = new PublishViewModel();

            switch (publishMode)
            {
                case PublishMode.DataQualityIndicator:

                    vm.PublishMode = PublishMode.DataQualityIndicator;
                    var validCourses = courses.Where(x => x.IsValid && ((int)x.CourseStatus & (int)RecordStatus.Live) > 0);
                    var results = _courseService.CourseValidationMessages(validCourses, ValidationMode.DataQualityIndicator).Value.ToList();
                    var invalidCoursesResult = results.Where(c => c.RunValidationResults.Any(cr => cr.Issues.Count() > 0));
                    var invalidCourses = invalidCoursesResult.Select(c => (Course)c.Course).ToList();
                    var invalidCourseRuns = invalidCourses.Select(cr => cr.CourseRuns.Where(x => x.StartDate < DateTime.Today));
                    List<Course> filteredList = new List<Course>();
                    var allRegions = _courseService.GetRegions().RegionItems;
                    foreach (var course in invalidCourses)
                    {
                        var invalidRuns = course.CourseRuns.Where(x => x.StartDate < DateTime.Today);
                        if (invalidRuns.Any())
                        {
                            course.CourseRuns = invalidRuns;
                            filteredList.Add(course);
                        }
                    }

                    if (invalidCourseRuns.Count() == 0 && courseId != null && courseRunId != null)
                    {
                        return BadRequest();
                    }

                    vm.NumberOfCoursesInFiles = invalidCourses.Count();
                    vm.Courses = filteredList.OrderBy(x => x.QualificationCourseTitle);
                    vm.Venues = VenueHelper.GetVenueNames(vm.Courses, _sqlQueryDispatcher).Result;
                    vm.Regions = allRegions;
                    break;
            }

            vm.NotificationTitle = notificationTitle;
            vm.CourseId = courseId;
            vm.CourseRunId = courseRunId;

            return View("Index", vm);
        }

        internal IEnumerable<Course> GetErrorMessages(IEnumerable<Course> courses, ValidationMode validationMode)
        {
            foreach (var course in courses)
            {
                course.ValidationErrors = _courseService.ValidateCourse(course).Select(x => x.Value);

                foreach (var courseRun in course.CourseRuns)
                {
                    courseRun.ValidationErrors = _courseService.ValidateCourseRun(courseRun, validationMode).Select(x => x.Value);
                }

                var crErrors = course.CourseRuns.Select(cr => cr.ValidationErrors.Count()).Sum();
                var cErrors = course.ValidationErrors.Count();

                if(crErrors > 0 || cErrors > 0)
                {
                    course.IsValid = false;
                }
            }
            return courses;
        }
    }
}
