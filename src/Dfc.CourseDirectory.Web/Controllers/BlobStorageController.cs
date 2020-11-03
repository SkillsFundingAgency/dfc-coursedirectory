using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.ApprenticeshipService;
using Dfc.CourseDirectory.Services.BlobStorageService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Services.Models.Providers;
using Dfc.CourseDirectory.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class BlobStorageController : Controller
    {
        private readonly ILogger<BlobStorageController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICourseService _courseService;
        private readonly IApprenticeshipService _apprenticeshipService;
        private readonly IBlobStorageService _blobService;
        private ICourseProvisionHelper _courseProvisionHelper;
        private IApprenticeshipProvisionHelper _apprenticeshipProvisionHelper;

        private ISession _session => _contextAccessor.HttpContext.Session;

        public BlobStorageController(
                ILogger<BlobStorageController> logger,
                IHttpContextAccessor contextAccessor,
                ICourseService courseService,
                IApprenticeshipService apprenticeshipService,
                IBlobStorageService blobService,
                ICourseProvisionHelper courseProvisionHelper,
                IApprenticeshipProvisionHelper apprenticeshipProvisionHelper)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(apprenticeshipService, nameof(apprenticeshipService));
            Throw.IfNull(blobService, nameof(blobService));
            Throw.IfNull(apprenticeshipProvisionHelper, nameof(apprenticeshipProvisionHelper));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            _blobService = blobService;
            _courseProvisionHelper = courseProvisionHelper;
            _apprenticeshipService = apprenticeshipService;
            _apprenticeshipProvisionHelper = apprenticeshipProvisionHelper;

        }

        [Authorize]
        public IActionResult Index()
        {
            int? UKPRN = _session.GetInt32("UKPRN");
            if (!UKPRN.HasValue)
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            //var vm = GetBlobStorageViewModel(_courseService, UKPRN, "");
            return View(); //vm);
        }

        public FileStreamResult GetBulkUploadTemplateFile()
        {
            MemoryStream ms = new MemoryStream();
            Task task = _blobService.GetBulkUploadTemplateFileAsync(ms);
            task.Wait();
            ms.Position = 0;
            FileStreamResult result = new FileStreamResult(ms, MediaTypeNames.Text.Plain);
            result.FileDownloadName = "bulk upload template.csv";
            return result;
        }

         public FileStreamResult GetApprenticeshipBulkUploadTemplateFile()
        {
            MemoryStream ms = new MemoryStream();
            Task task = _blobService.GetBulkUploadTemplateFileAsync(ms,ProviderType.Apprenticeship);
            task.Wait();
            ms.Position = 0;
            FileStreamResult result = new FileStreamResult(ms, MediaTypeNames.Text.Plain);
            result.FileDownloadName = "apprenticeships bulk upload template.csv";
            return result;
        }
        public FileStreamResult GetCurrentCoursesTemplateFile()
        {
            if (!_session.GetInt32("UKPRN").HasValue)
            {
                return null;
            }

            return _courseProvisionHelper.DownloadCurrentCourseProvisions();
        }
        public FileStreamResult GetCurrentApprenticeshipsTemplateFile()
        {
            if (!_session.GetInt32("UKPRN").HasValue)
            {
                return null;
            }

            return _apprenticeshipProvisionHelper.DownloadCurrentApprenticeshipProvisions();
        }
        [Authorize]
        public FileStreamResult GetBulkUploadErrors(int? UKPRN)
        {
            if (!UKPRN.HasValue)
                return null;

            IEnumerable<Course> courses = _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
                                                        .Result
                                                        .Value
                                                        .Value
                                                        .SelectMany(o => o.Value)
                                                        .SelectMany(i => i.Value)
                                                        .Where((y => ((int)y.CourseStatus & (int)RecordStatus.BulkUploadPending) > 0
                                                        || ((int)y.CourseStatus & (int)RecordStatus.BulkUploadReadyToGoLive) > 0));

            var courseBUErrors = courses.Where(x => x.BulkUploadErrors != null).SelectMany(y => y.BulkUploadErrors).ToList();
            var courseRunsBUErrors = courses.SelectMany(x => x.CourseRuns.Where(y => y.BulkUploadErrors != null).SelectMany(y => y.BulkUploadErrors)).ToList();
            var totalErrorList = courseBUErrors.Union(courseRunsBUErrors).OrderBy(x => x.LineNumber);


            IEnumerable<string> headers = new string[] { "Row Number,Column Name,Error Description" };
            IEnumerable<string> csvlines = totalErrorList.Select(i => string.Join(",", new string[] { i.LineNumber.ToString(), i.Header, i.Error.Replace(',', ' ') }));
            string report = string.Join(Environment.NewLine, headers.Concat(csvlines));
            byte[] data = Encoding.ASCII.GetBytes(report);
            MemoryStream ms = new MemoryStream(data)
            {
                Position = 0
            };

            FileStreamResult result = new FileStreamResult(ms, MediaTypeNames.Text.Plain);
            DateTime d = DateTime.Now;
            result.FileDownloadName = $"Bulk_upload_errors_{UKPRN}_{d.Day.TwoChars()}_{d.Month.TwoChars()}_{d.Year}_{d.Hour.TwoChars()}_{d.Minute.TwoChars()}.csv";
            return result;
        }
        [Authorize]
        public FileStreamResult GetApprenticeshipBulkUploadErrors(int? UKPRN)
        {
            if (!UKPRN.HasValue)
                return null;

            var apprenticeships = _apprenticeshipService.GetApprenticeshipByUKPRN(UKPRN.ToString())
                .Result.Value.Where((y => ((int)y.RecordStatus & (int)RecordStatus.BulkUploadPending) > 0
                                          || ((int)y.RecordStatus & (int)RecordStatus.BulkUploadReadyToGoLive) > 0));
            //.Where((y => ((int)y. & (int)RecordStatus.BulkUploadPending) > 0
                                                        //|| ((int)y.CourseStatus & (int)RecordStatus.BulkUploadReadyToGoLive) > 0));

            var apprenticeshipBUErrors = apprenticeships.Where(x => x.BulkUploadErrors != null).SelectMany(y => y.BulkUploadErrors).ToList();


            IEnumerable<string> headers = new string[] { "Row Number,Column Name,Error Description" };
            IEnumerable<string> csvlines = apprenticeshipBUErrors.Select(i => string.Join(",", new string[] { i.LineNumber.ToString(), i.Header, i.Error.Replace(',', ' ') }));
            string report = string.Join(Environment.NewLine, headers.Concat(csvlines));
            byte[] data = Encoding.ASCII.GetBytes(report);
            MemoryStream ms = new MemoryStream(data)
            {
                Position = 0
            };

            FileStreamResult result = new FileStreamResult(ms, MediaTypeNames.Text.Plain);
            DateTime d = DateTime.Now;
            result.FileDownloadName = $"Bulk_upload_errors_{UKPRN}_{d.Day.TwoChars()}_{d.Month.TwoChars()}_{d.Year}_{d.Hour.TwoChars()}_{d.Minute.TwoChars()}.csv";
            return result;
        }
}
    internal static class TwoCharsClass
    {
        internal static string TwoChars(this int extendee)
        {
            return extendee.ToString().Length < 2 ? $"0{extendee.ToString()}" : extendee.ToString();
        }
    }



}
