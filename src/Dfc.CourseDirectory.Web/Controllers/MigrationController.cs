using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.ViewModels.Migration;
using Dfc.CourseDirectory.WebV2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    [RequireProviderContext]
    public class MigrationController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IVenueService _venueService;
        private readonly ISearchClient<Lars> _larsSearchClient;

        private ISession Session => HttpContext.Session;

        public MigrationController(
            ICourseService courseService,
            IVenueService venueService,
            ISearchClient<Lars> larsSearchClient)
        {
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _venueService = venueService ?? throw new ArgumentNullException(nameof(venueService));
            _larsSearchClient = larsSearchClient ?? throw new ArgumentNullException(nameof(larsSearchClient));
        }

        [Authorize]
        public IActionResult Index()
        {
            Session.SetString("Option", "Migration");
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
        public IActionResult Options(OptionsViewModel viewModel)
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

        public IActionResult Errors(ErrorsViewModel model)
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
            var ukprn = Session.GetInt32("UKPRN").Value;
            var courseCounts = await _courseService.GetCourseCountsByStatusForUKPRN(new CourseSearchCriteria(ukprn));
            var courseErrors = courseCounts.Value.SingleOrDefault(x => x.Status == (int)RecordStatus.MigrationPending);
            var model = new DeleteViewModel { CourseErrors = courseErrors?.Count };

            return View("../Migration/Delete/Index", model);
        }

        [Authorize("Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(DeleteViewModel model)
        {

            int? sUKPRN = Session.GetInt32("UKPRN");
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
                    await _courseService.ArchiveCourseRunsByUKPRN(UKPRN);
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
            int? sUKPRN = Session.GetInt32("UKPRN");
            if (!sUKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var course = await _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(courseId));

            if (!course.IsSuccess)
            {
                throw new Exception($"Unable to find Course with id {courseId}");
            }

            var courseRun = course.Value.CourseRuns.SingleOrDefault(x => x.id == courseRunId);

            if (courseRun == null) throw new Exception($"Unable to find course run:{courseRunId} for course: {course.Value.id}");

            var result = await _courseService.UpdateStatus(courseId, courseRunId, (int)RecordStatus.Archived);

            if (!result.IsSuccess)
            {
                throw new Exception($"Unable to delete Course run with id {courseRunId}");
            }

            return View("CourseRunDeleted/index", new DeleteCourseRunViewModel() {  CourseName = courseRun.CourseName});
        }

        [HttpGet]
        public async Task<IActionResult> Report()
        {
            var ukprn = Session.GetInt32("UKPRN");
            if (ukprn == null)
            {
                throw new Exception("UKPRN is null");
            }

            var courses = await _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(ukprn));

            if (!courses.IsSuccess)
            {
                throw new Exception($"Unable to find courses for UKPRN: {ukprn}");
            }

            var result = await _courseService.GetCourseMigrationReport(ukprn.Value);

            var model = new ReportViewModel(
                courses.Value.Value.SelectMany(o => o.Value).SelectMany(i => i.Value),
                result.Value);

            return View("Report/index", model);
        }

        [HttpGet]
        public async Task<IActionResult> Larsless()
        {
            var larlessErrors = new Dictionary<string, string>();
            var ukprn = Session.GetInt32("UKPRN");
            var courseMigrationReport = await _courseService.GetCourseMigrationReport(ukprn.Value);

            if (!courseMigrationReport.IsSuccess)
            {
                throw new Exception(courseMigrationReport.Error + $"For UKPRN: {ukprn}");
            }

            var venues = await VenueHelper.GetVenueNames(courseMigrationReport.Value.LarslessCourses, _venueService);

            var model = new LarslessViewModel
            {
                LarslessCourses = courseMigrationReport.Value
                    .LarslessCourses
                    .OrderBy(x => x.QualificationCourseTitle),
                Venues = venues
            };

            var errors = new List<string>();
            var totalErrorList = new List<int>();

            var cachedQuals = new List<Lars>();

            foreach (var bulkUploadcourse in model.LarslessCourses)
            {
                if (!string.IsNullOrEmpty(bulkUploadcourse.LearnAimRef))
                {
                    var cachedResult = cachedQuals.FirstOrDefault(o => o.LearnAimRef == bulkUploadcourse.LearnAimRef);

                    List<Lars> results = null;

                    if (cachedResult == null)
                    {
                        results = _larsSearchClient.Search(new LarsLearnAimRefSearchQuery
                        {
                            LearnAimRef = bulkUploadcourse.LearnAimRef
                        }).GetAwaiter().GetResult().Items.Select(r => r.Record).ToList();

                        var qual = results.FirstOrDefault();

                        if (qual != null)
                        {
                            cachedQuals.Add(qual);
                        }
                    }
                    else
                    {
                        results = new List<Lars> { cachedQuals.FirstOrDefault(o => o.LearnAimRef == bulkUploadcourse.LearnAimRef) };
                    }

                    if (results.Count > 0)
                    {
                        if (results[0].CertificationEndDate != null && results[0].CertificationEndDate < DateTime.Now)
                        {
                            larlessErrors.Add(bulkUploadcourse.CourseId.Value.ToString(), "Expired LARS Code " + bulkUploadcourse.LearnAimRef);
                        }
                    }
                    else
                    {
                        larlessErrors.Add(bulkUploadcourse.CourseId.Value.ToString(), "Unrecognised LARS Code " + bulkUploadcourse.LearnAimRef);
                    }
                }
                else
                {
                    larlessErrors.Add(bulkUploadcourse.CourseId.Value.ToString(), "Missing LARS");
                }
            }

            model.Errors = larlessErrors;
            return View("Report/larsless", model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult MigrationReportFoProvider(string UKPRN)
        {
            Session.SetInt32("UKPRN", Convert.ToInt32(UKPRN));
            return RedirectToAction("Report", "Migration");
        }
    }
}
