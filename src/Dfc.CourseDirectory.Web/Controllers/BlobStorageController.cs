using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Services.ApprenticeshipService;
using Dfc.CourseDirectory.Services.BlobStorageService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Apprenticeships;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class BlobStorageController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IApprenticeshipService _apprenticeshipService;
        private readonly IBlobStorageService _blobService;
        private ICourseProvisionHelper _courseProvisionHelper;
        private IApprenticeshipProvisionHelper _apprenticeshipProvisionHelper;

        private ISession _session => HttpContext.Session;

        public BlobStorageController(
                ICourseService courseService,
                IApprenticeshipService apprenticeshipService,
                IBlobStorageService blobService,
                ICourseProvisionHelper courseProvisionHelper,
                IApprenticeshipProvisionHelper apprenticeshipProvisionHelper)
        {
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
            _courseProvisionHelper = courseProvisionHelper ?? throw new ArgumentNullException(nameof(courseProvisionHelper));
            _apprenticeshipService = apprenticeshipService ?? throw new ArgumentNullException(nameof(apprenticeshipService));
            _apprenticeshipProvisionHelper = apprenticeshipProvisionHelper ?? throw new ArgumentNullException(nameof(apprenticeshipProvisionHelper));
        }

        [Authorize]
        public IActionResult Index()
        {
            if (!_session.GetInt32("UKPRN").HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            return View();
        }

        public async Task<FileStreamResult> GetBulkUploadTemplateFile()
        {
            var ms = new MemoryStream();
            await _blobService.GetBulkUploadTemplateFileAsync(ms);

            ms.Position = 0;

            return new FileStreamResult(ms, MediaTypeNames.Text.Plain)
            {
                FileDownloadName = "bulk upload template.csv"
            };
        }

        public FileStreamResult GetApprenticeshipBulkUploadTemplateFile()
        {
            var stream = new MemoryStream();

            using (var writer = new StreamWriter(stream, leaveOpen: true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteHeader<CsvApprenticeship>();
            }

            stream.Position = 0;

            return new FileStreamResult(stream, MediaTypeNames.Text.Plain)
            {
                FileDownloadName = "apprenticeships bulk upload template.csv"
            };
        }

        public Task<FileStreamResult> GetCurrentCoursesTemplateFile()
        {
            if (!_session.GetInt32("UKPRN").HasValue)
            {
                return null;
            }

            return _courseProvisionHelper.DownloadCurrentCourseProvisions();
        }

        public Task<FileStreamResult> GetCurrentApprenticeshipsTemplateFile()
        {
            if (!_session.GetInt32("UKPRN").HasValue)
            {
                return Task.FromResult<FileStreamResult>(null);
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
