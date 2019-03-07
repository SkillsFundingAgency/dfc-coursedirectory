using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.ViewModels.PublishCourses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers.PublishCourses
{

    public class PublishCoursesController : Controller
    {
        private readonly ILogger<PublishCoursesController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly ICourseService _courseService;

        public PublishCoursesController(ILogger<PublishCoursesController> logger,
            IHttpContextAccessor contextAccessor, ICourseService courseService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(courseService, nameof(courseService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Index(PublishMode publishMode)
        {

            PublishViewModel vm = new PublishViewModel();

            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            List<Course> Courses = new List<Course>();

            ICourseSearchResult coursesByUKPRN = (!UKPRN.HasValue
                   ? null
                   : _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
                       .Result.Value);
            Courses = coursesByUKPRN.Value.SelectMany(o => o.Value).SelectMany(i => i.Value).ToList();

            var migratedCourses = Courses.Where(x => x.CourseRuns.Any(cr => cr.RecordStatus == RecordStatus.MigrationPending || cr.RecordStatus == RecordStatus.MigrationReadyToGoLive)).ToList();
            var bulkUploadedCourses = Courses.Where(x => x.CourseRuns.Any(cr => cr.RecordStatus == RecordStatus.BulkUloadPending || cr.RecordStatus == RecordStatus.BulkUploadReadyToGoLive)).ToList();
            var liveCourses = Courses.Where(x => x.CourseRuns.Any(cr => cr.RecordStatus == RecordStatus.Live)).ToList();

            switch (publishMode)
            {
                case PublishMode.Migration:
                    //TODO replace with call to service to return by status
                    vm.PublishMode = PublishMode.Migration;
                    vm.NumberOfCoursesInFiles = 10;
                    //vm.Courses = Courses;
                    vm.Courses = migratedCourses;

                    break;
                case PublishMode.BulkUpload:

                    //TODO replace with call to service to return by status
                    vm.PublishMode = PublishMode.BulkUpload;
                    vm.NumberOfCoursesInFiles = 10;
                    //vm.Courses = Courses;
                    vm.Courses = bulkUploadedCourses;

                    break;
                    }

                    return View("Index", vm);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Index(PublishViewModel vm)
        {
            PublishCompleteViewModel CompleteVM = new PublishCompleteViewModel();

            int? UKPRN = _session.GetInt32("UKPRN");
            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }
            //Archive any existing courses
            var archiveExistingCourse = await _courseService.ArchiveProviderLiveCourses(UKPRN);

            foreach (var course in vm.Courses)
            {
                course.IsValid = true;

                foreach (var courseRuns in course.CourseRuns)
                {
                    courseRuns.RecordStatus = RecordStatus.Live;
                }

                // AM - We don't add courses. We modify them.
                //var result = await _courseService.AddCourseAsync(course);

                //if (result.IsSuccess && result.HasValue)
                //{
                //    CompleteVM.NumberOfCoursesPublished++;
                //}
            }
            return View("Complete", CompleteVM);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Delete(Guid courseId, Guid courseRunId)
        {
            //TODO delete

            return RedirectToAction("Index", "PublishCourses", new { publishMode = PublishMode.Migration });
        }
    }
}