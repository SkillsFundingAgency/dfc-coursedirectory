using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.Helpers.Attributes;
using Dfc.CourseDirectory.Web.ViewModels.Migration;
using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;

namespace Dfc.CourseDirectory.Web.Controllers
{
    [SelectedProviderNeeded]
    public class MigrationController : Controller
    {
        private readonly ILogger<MigrationController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IBulkUploadService _bulkUploadService;
        private readonly IUserHelper _userHelper;
        private readonly ICourseService _courseService;
        private readonly IVenueService _venueService;

        private IHostingEnvironment _env;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public MigrationController(
            ILogger<MigrationController> logger,
            IHttpContextAccessor contextAccessor,
            IBulkUploadService bulkUploadService,
            IHostingEnvironment env, IUserHelper userHelper, ICourseService courseService, IVenueService venueService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(bulkUploadService, nameof(bulkUploadService));
            Throw.IfNull(env, nameof(env));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _bulkUploadService = bulkUploadService;
            _env = env;
            _userHelper = userHelper;
            _courseService = courseService;
            _venueService = venueService;
        }


        [Authorize]
        public IActionResult Index()
        {
            _session.SetString("Option", "Migration");
            return RedirectToAction("Index", "PublishCourses", new { publishMode = PublishMode.Migration });
        }

        [Authorize]
        [HttpGet]
        public IActionResult Options()
        {
            return View("../Migration/Options/Index");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Options(OptionsViewModel viewModel)
        {
            switch (viewModel.MigrationOption)
            {
                case MigrationOptions.CheckCourses:
                    return RedirectToAction("Index", "ProviderCourses");
                case MigrationOptions.StartAgain:
                    return RedirectToAction("Index", "BulkUpload");
                default:
                    return RedirectToAction("Options", "Migration");
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult Errors(int? liveCourses, int? errors)
        {
            var model = new ErrorsViewModel
            {
                LiveCourses = liveCourses,
                Errors = errors
            };

            return View("../Migration/Errors/Index", model);
        }

        [Authorize]
        [HttpPost]

        public async Task<IActionResult> Errors(ErrorsViewModel model)
        {
            switch (model.MigrationErrors)
            {
                case MigrationErrors.FixErrors:
                    return RedirectToAction("Index", "PublishCourses", new { publishMode = PublishMode.Migration });
                case MigrationErrors.DeleteCourses:
                    return RedirectToAction("Index", "HelpDesk");
                case MigrationErrors.StartAgain:
                    return RedirectToAction("Index", "BulkUpload");
                default:
                    return RedirectToAction("Errors");
            }
        }

        [Authorize("Admin")]
        public async Task<IActionResult> Delete()
        {
            var ukprn = _session.GetInt32("UKPRN").Value;
            var courseCounts = await _courseService.GetCourseCountsByStatusForUKPRN(new CourseSearchCriteria(ukprn));
            var courseErrors = courseCounts.Value.SingleOrDefault(x => x.Status == (int)RecordStatus.MigrationPending);
            var model = new DeleteViewModel { CourseErrors = courseErrors?.Count };

            return View("../Migration/Delete/Index", model);
        }

        [Authorize("Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(DeleteViewModel model)
        {

            int? sUKPRN = _session.GetInt32("UKPRN");
            int UKPRN;
            if (!sUKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }
            else
            {
                UKPRN = sUKPRN ?? 0;
            }

            switch (model.MigrationDeleteOptions)
            {
                case MigrationDeleteOptions.DeleteMigrations:
                    await _courseService.ChangeCourseRunStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.MigrationPending, (int)RecordStatus.Archived);
                    return View("../Migration/DeleteConfirmed/Index");
                case MigrationDeleteOptions.Cancel:
                    return RedirectToAction("Index", "PublishCourses", new { publishMode = PublishMode.Migration });
                default:
                    return RedirectToAction("index");
            }
        }
        
        [HttpGet]
        [Authorize("Admin")]
        public async Task<IActionResult> DeleteCourseRun(Guid courseId, Guid courseRunId)
        {
            int? sUKPRN = _session.GetInt32("UKPRN");
            int UKPRN;
            if (!sUKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var course = await _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(courseId));
            
            if (course.IsFailure || !course.HasValue) throw new Exception($"Unable to find Course with id {courseId}");

            var courseRun = course.Value.CourseRuns.SingleOrDefault(x => x.id == courseRunId);

            if (courseRun == null) throw new Exception($"Unable to find course run:{courseRunId} for course: {course.Value.id}");

            var result = await _courseService.UpdateStatus(courseId, courseRunId, (int)RecordStatus.Archived);

            if (result.IsFailure) throw new Exception($"Unable to delete Course run with id {courseRunId}");

            if (course.Value.CourseRuns.Any(x => x.id != courseRunId && x.RecordStatus == RecordStatus.Pending))
            {
                return RedirectToAction("Index", "PublishCourses", new
                {
                    publishMode = PublishMode.Migration,
                    notificationTitle = $"{courseRun.CourseName} was successfully deleted"
                });
            }

            return View("Complete/index");
        }

        [HttpGet]
        public async Task<IActionResult> Report()
        {
            var ukprn = _session.GetInt32("UKPRN");
            if (ukprn == null)
            {
                throw new Exception("UKPRN is null");
            }

            var courses = await _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(ukprn));
            

            if (courses.IsFailure)
            {
                throw new Exception($"Unable to find courses for UKPRN: {ukprn}");
            }

            var courseMigrationReport = await _courseService.GetCourseMigrationReport(ukprn.Value);
            if (courseMigrationReport.IsFailure)
            {
                throw new Exception(courseMigrationReport.Error + $"For UKPRN: {ukprn}");
            }

            var model = new ReportViewModel(courses.Value.Value.SelectMany(o => o.Value).SelectMany(i => i.Value)
                ,courseMigrationReport.Value);

            return View("Report/index", model);
        }

        [HttpGet]
        public async Task<IActionResult> Larsless()
        {
            var ukprn = _session.GetInt32("UKPRN");
            var courseMigrationReport = await _courseService.GetCourseMigrationReport(ukprn.Value);
            if (courseMigrationReport.IsFailure)
            {
                throw new Exception(courseMigrationReport.Error + $"For UKPRN: {ukprn}");
            }

            var venues = await VenueHelper.GetVenueNames(courseMigrationReport.Value.LarslessCourses, _venueService);
        }
    }
}