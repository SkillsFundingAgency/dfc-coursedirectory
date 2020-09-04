
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;
using Dfc.CourseDirectory.Web.Helpers;


namespace Dfc.CourseDirectory.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICourseService _courseService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IEnvironmentHelper _environmentHelper;
        private IWebHostEnvironment _env;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public DashboardController(
                ILogger<DashboardController> logger,
                IHttpContextAccessor contextAccessor,
                ICourseService courseService,
                IBlobStorageService blobStorageService,
                IWebHostEnvironment env,
                IEnvironmentHelper environmentHelper)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(blobStorageService, nameof(blobStorageService));
            Throw.IfNull(env, nameof(env));
            Throw.IfNull(environmentHelper, nameof(environmentHelper));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            _blobStorageService = blobStorageService;
            _env = env;
            _environmentHelper = environmentHelper;
        }

        public static DashboardViewModel GetDashboardViewModel(ICourseService service, IBlobStorageService blobStorageService, int? UKPRN, string successHeader)
        {
            if (!UKPRN.HasValue)
                return new DashboardViewModel();

            IEnumerable<Course> courses = service.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
                                                 .Result
                                                 .Value
                                                 .Value
                                                 .SelectMany(o => o.Value)
                                                 .SelectMany(i => i.Value);

            int[] pendingStatuses = new int[] { (int)RecordStatus.Pending, (int)RecordStatus.BulkUploadPending, (int)RecordStatus.APIPending, (int)RecordStatus.MigrationPending, (int)RecordStatus.MigrationReadyToGoLive, (int)RecordStatus.BulkUploadReadyToGoLive };
            int[] bulkStatuses = new int[] { (int)RecordStatus.BulkUploadPending };
            int[] migrationStatuses = new int[] { (int)RecordStatus.MigrationPending };
            IEnumerable<Course> validCourses = courses.Where(c => c.IsValid);

            IEnumerable<CourseRun> all = courses.SelectMany(c => c.CourseRuns);
            IEnumerable<CourseRun> live =                       courses.SelectMany(c => c.CourseRuns)
                                                                       .Where(x => x.RecordStatus == RecordStatus.Live);
            IEnumerable<Course>    bulkUploadCoursesPending =   courses.Where(x => ((int)x.CourseStatus & (int)RecordStatus.BulkUploadPending) > 0);
            IEnumerable<CourseRun> bulkUploadRunsPending =      courses.SelectMany(c => c.CourseRuns)
                                                                       .Where(x => x.RecordStatus == RecordStatus.BulkUploadPending);
            IEnumerable<CourseRun> migrationPending =           courses.SelectMany(c => c.CourseRuns)
                                                                       .Where(x => x.RecordStatus == RecordStatus.MigrationPending);
            IEnumerable<CourseRun> bulkUploadReadyToGoLive =    courses.SelectMany(c => c.CourseRuns)
                                                                       .Where(x => x.RecordStatus == RecordStatus.BulkUploadReadyToGoLive);
            IEnumerable<CourseValidationResult> results =       service.CourseValidationMessages(validCourses.Where(x => ((int)x.CourseStatus & (int)RecordStatus.Live) > 0),
                                                                                                 ValidationMode.DataQualityIndicator)
                                                                       .Value;

            IEnumerable<string> courseMessages   =        results.SelectMany(c => c.Issues);
            IEnumerable<string> runMessages      =        results.SelectMany(c => c.RunValidationResults)
                                                                 .SelectMany(r => r.Issues);
            IEnumerable<string> messages         = courseMessages.Concat(runMessages)
                                                                 .GroupBy(i => i)
                                                                 .Select(g => $"{ g.LongCount() } { g.Key }");

            IEnumerable<ICourseStatusCountResult> counts = service.GetCourseCountsByStatusForUKPRN(new CourseSearchCriteria(UKPRN))
                                                                  .Result
                                                                  .Value;
            DashboardViewModel vm = new DashboardViewModel()
            {
                SuccessHeader = successHeader,
                ValidationHeader = $"{ courseMessages.LongCount() + runMessages.LongCount() } data items require attention",
                ValidationMessages = messages,
                LiveCourseCount         = courses.Where(x => x.CourseStatus == RecordStatus.Live)
                                                 .SelectMany(c => c.CourseRuns)
                                                 .Where(x => x.RecordStatus == RecordStatus.Live)
                                                 .Count(),
                ArchivedCourseCount     =  counts.FirstOrDefault(c => c.Status == (int)RecordStatus.Archived)
                                                 .Count,
                MigrationPendingCount   = courses.SelectMany(c => c.CourseRuns)
                                                 .Where(x => x.RecordStatus == RecordStatus.MigrationPending)
                                                 .Count(),
                PendingCourseCount = (from ICourseStatusCountResult c in counts
                                      join int p in pendingStatuses
                                      on c.Status equals p
                                      select c.Count).Sum(),
                BulkUploadPendingCount = bulkUploadRunsPending.Count(), // = bulkUploadCoursesPending.Count()
                BulkUploadReadyToGoLiveCount = bulkUploadReadyToGoLive.Count(),
                BulkUploadTotalCount = bulkUploadCoursesPending.Count()+ bulkUploadReadyToGoLive.Count()
            };

            IEnumerable<Services.BlobStorageService.BlobFileInfo> list = blobStorageService.GetFileList(UKPRN + "/Bulk Upload/Files/").OrderByDescending(x => x.DateUploaded).ToList();
            if (list.Any())
                vm.FileUploadDate = list.FirstOrDefault().DateUploaded.Value;

            //string BulkUpLoadErrorMessage = vm.BulkUploadTotalCount.ToString() + WebHelper.GetCourseTextToUse(vm.BulkUploadTotalCount) + " upload in a file on " + vm.FileUploadDate?.ToString("dd/MM/yyyy") + " have " + vm.BulkUploadPendingCount.ToString() + " errors. Fix these to publish all of your courses.";
            string BulkUpLoadErrorMessage = vm.BulkUploadPendingCount.ToString() + WebHelper.GetCourseTextToUse(vm.BulkUploadTotalCount) + " upload in a file on "
                                                    + vm.FileUploadDate?.ToString("dd/MM/yyyy") + " have "
                                                    + (bulkUploadCoursesPending.SelectMany(c => c.BulkUploadErrors).Count() + bulkUploadRunsPending.SelectMany(r => r.BulkUploadErrors).Count()).ToString()
                                                    + " errors. Fix these to publish all of your courses.";
            //string BulkUpLoadNoErrorMessage = vm.BulkUploadTotalCount.ToString() + WebHelper.GetCourseTextToUse(vm.BulkUploadPendingCount) + " uploaded on " + vm.FileUploadDate?.ToString("dd/MM/yyyy") + " have no errors, but are not listed on the Course directory because you have not published them.";

            string BulkUpLoadNoErrorMessage = "Your bulk upload is complete." + vm.BulkUploadTotalCount.ToString() + WebHelper.GetCourseTextToUse(vm.BulkUploadPendingCount) + " have been uploaded on " + vm.FileUploadDate?.ToString("dd/MM/yyyy") + " and ready to publish to the course directory.";
            vm.FileCount = list.Count();

            vm.FileCount = list.Count();

            int MigrationLiveCount = courses.Where(x => x.CourseStatus == RecordStatus.Live && x.CreatedBy == "DFC – Course Migration Tool")
                                            .SelectMany(c => c.CourseRuns)
                                            .Where(x => x.RecordStatus == RecordStatus.Live && x.CreatedBy == "DFC – Course Migration Tool")
                                            .Count();
            int totalCourses = MigrationLiveCount + vm.MigrationPendingCount;

            vm.BulkUploadMessage = (vm.BulkUploadTotalCount > 0 & vm.BulkUploadPendingCount == 0) ? BulkUpLoadNoErrorMessage : BulkUpLoadErrorMessage;
            vm.MigrationErrorMessage = totalCourses.ToString() + WebHelper.GetCourseTextToUse(totalCourses) + " have been migrated to the new Course directory. You have " + vm.MigrationPendingCount.ToString() + WebHelper.GetCourseTextToUse(vm.MigrationPendingCount) + " with errors and these must be fixed before they can be published.";
            vm.MigrationOKMessage = MigrationLiveCount.ToString() + WebHelper.GetCourseTextToUse(MigrationLiveCount) + " have been migrated to the new Course directory. Any courses with a missing LARS have been deleted.";
            
            return vm;
        }

        [Authorize]
        public IActionResult Index()
        {
            //_session.SetString("Option", "Dashboard");
            //return RedirectToAction("Index", "PublishCourses", new { publishMode = PublishMode.Migration });
            int? UKPRN = _session.GetInt32("UKPRN");
            if (!UKPRN.HasValue)
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            var vm = GetDashboardViewModel(_courseService, _blobStorageService,UKPRN, "");
            if (vm.PendingCourseCount > 0)
                _session.SetString("PendingCourses", "true");
            else
                _session.SetString("PendingCourses", "false");
            vm.EnvironmentType = _environmentHelper.GetEnvironmentType();

            return View(vm);
        }
    }
}