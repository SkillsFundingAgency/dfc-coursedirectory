
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.BlobStorageService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using System.Text.RegularExpressions;
using System.Reflection;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Models.Models.Regions;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Services.ProviderService;
using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Web.Controllers
{

    public class BlobStorageController : Controller
    {
        private readonly ILogger<BlobStorageController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICourseService _courseService;
        private readonly IBlobStorageService _blobService;
        private readonly IVenueService _venueService;
        private readonly IProviderService _providerService;

        //private IHostingEnvironment _env;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public BlobStorageController(
                ILogger<BlobStorageController> logger,
                IHttpContextAccessor contextAccessor,
                ICourseService courseService,
                IBlobStorageService blobService,
                IVenueService venueService,
                IProviderService providerService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(venueService, nameof(venueService));
            Throw.IfNull(blobService, nameof(blobService));
            Throw.IfNull(providerService, nameof(providerService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            _venueService = venueService;
            _blobService = blobService;
            _providerService = providerService;
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
        public FileStreamResult GetCurrentCoursesTemplateFile()
        {
            int? UKPRN = null;
            IProviderSearchResult providerSearchResult = null;
            string providerName = String.Empty;
            if (_session.GetInt32("UKPRN").HasValue)
            {

                UKPRN = _session.GetInt32("UKPRN").Value;
                providerSearchResult = _providerService.GetProviderByPRNAsync(new Services.ProviderService.ProviderSearchCriteria(UKPRN.Value.ToString())).Result.Value;
                providerName = providerSearchResult.Value.FirstOrDefault()?.ProviderName.Replace(" ", "");
            }
            if (!UKPRN.HasValue)
                return null;

            IEnumerable<Course> courses = _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
                                            .Result
                                            .Value
                                            .Value
                                            .SelectMany(o => o.Value)
                                            .SelectMany(i => i.Value)
                                            .Where((y => ((int)y.CourseStatus & (int)RecordStatus.Live) > 0));


            //for each course, convert to CsvCourse
            List<CsvCourse> csvCourses = new List<CsvCourse>();
            foreach (var course in courses)
            {
                //First course run is on same line as course line
                var firstCourseRun = course.CourseRuns.First();

                SelectRegionModel selectRegionModel = new SelectRegionModel();

                CsvCourse csvCourse = new CsvCourse
                {
                    LearnAimRef = course.LearnAimRef,
                    CourseDescription = course.CourseDescription.Replace(",", " "),
                    EntryRequirements = course.EntryRequirements.Replace(",", " "),
                    WhatYoullLearn = course.WhatYoullLearn.Replace(",", " "),
                    HowYoullLearn = course.HowYoullLearn.Replace(",", " "),
                    WhatYoullNeed = course.WhatYoullNeed.Replace(",", " "),
                    HowYoullBeAssessed = course.HowYoullBeAssessed.Replace(",", " "),
                    WhereNext = course.WhereNext.Replace(",", " "),
                    AdvancedLearnerLoan = course.AdvancedLearnerLoan ? "Yes" : "No",
                    AdultEducationBudget = course.AdultEducationBudget ? "Yes" : "No",
                    CourseName = firstCourseRun.CourseName.Replace(",", " "),
                    ProviderCourseID = firstCourseRun.ProviderCourseID,
                    DeliveryMode = firstCourseRun.DeliveryMode,
                    StartDate = firstCourseRun.StartDate,
                    FlexibleStartDate = firstCourseRun.FlexibleStartDate ? "Yes" : string.Empty,
                    VenueName = firstCourseRun.VenueId.HasValue ? _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(firstCourseRun.VenueId.Value.ToString())).Result.Value.VenueName : null,
                    National = firstCourseRun.National.HasValue ? (firstCourseRun.National.Value ? "Yes" : "No") : string.Empty,
                    Regions = firstCourseRun.Regions != null ? SemiColonSplit(
                                                                selectRegionModel.RegionItems
                                                                .Where(x => firstCourseRun.Regions.Contains(x.Id))
                                                                .Select(y => y.RegionName).ToList()) 
                                                                : null,
                    SubRegions = firstCourseRun.SubRegions != null? SemiColonSplit(firstCourseRun.SubRegions.Select(x => x.SubRegionName).ToList()) : null,
                    CourseURL = firstCourseRun.CourseURL,
                    Cost = firstCourseRun.Cost,
                    CostDescription = firstCourseRun.CostDescription.Replace(",", " "),
                    DurationValue = firstCourseRun.DurationValue,
                    DurationUnit = firstCourseRun.DurationUnit,
                    StudyMode = firstCourseRun.StudyMode,
                    AttendancePattern = firstCourseRun.AttendancePattern
                };
                csvCourses.Add(csvCourse);
                foreach (var courseRun in course.CourseRuns)
                {
                    //Ignore the first course run as we've already captured it
                    if(courseRun.id == firstCourseRun.id)
                    {
                        continue;
                    }

                    CsvCourse csvCourseRun = new CsvCourse
                    {
                        CourseName = courseRun.CourseName,
                        ProviderCourseID = courseRun.ProviderCourseID,
                        DeliveryMode = courseRun.DeliveryMode,
                        StartDate = courseRun.StartDate,
                        FlexibleStartDate = courseRun.FlexibleStartDate ? "Yes" : string.Empty,
                        VenueName = courseRun.VenueId.HasValue ? _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(courseRun.VenueId.Value.ToString())).Result.Value.VenueName : null,
                        National = courseRun.National.HasValue ? (firstCourseRun.National.Value ? "Yes" : "No") : string.Empty,
                        Regions = courseRun.Regions != null ? SemiColonSplit(
                                                                selectRegionModel.RegionItems
                                                                .Where(x => courseRun.Regions.Contains(x.Id))
                                                                .Select(y => y.RegionName).ToList())
                                                                : null,
                        SubRegions = courseRun.SubRegions != null ? SemiColonSplit(courseRun.SubRegions.Select(x => x.SubRegionName).ToList()) : null,
                        CourseURL = courseRun.CourseURL,
                        Cost = courseRun.Cost,
                        CostDescription = courseRun.CostDescription.Replace(",", " "),
                        DurationValue = courseRun.DurationValue,
                        DurationUnit = courseRun.DurationUnit,
                        StudyMode = courseRun.StudyMode,
                        AttendancePattern = courseRun.AttendancePattern
                    };
                    csvCourses.Add(csvCourseRun);
                }
            }
            //foreach courseRun of course, convert to csvCourse
            //push to csv
            List<string> csvLines = new List<string>();
            foreach (var line in ToCsv(csvCourses))
            {
                csvLines.Add(line);
            }
            string report = string.Join(Environment.NewLine, csvLines);
            byte[] data = Encoding.ASCII.GetBytes(report);
            MemoryStream ms = new MemoryStream(data)
            {
                Position = 0
            };
            FileStreamResult result = new FileStreamResult(ms, MediaTypeNames.Text.Plain);
            DateTime d = DateTime.Now;
            result.FileDownloadName = $"{providerName}_Courses_{d.Day.TwoChars()}_{d.Month.TwoChars()}_{d.Year}_{d.Hour.TwoChars()}_{d.Minute.TwoChars()}.csv";
            return result;
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

        internal static IEnumerable<string> ToCsv<T>(IEnumerable<T> objectlist, string separator = ",", bool header = true)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            if (header)
            {
                yield return String.Join(separator, properties.Select(p => p.CustomAttributes.Select(x => x.NamedArguments.Select(y => y.TypedValue.Value.ToString()))));
            }
            foreach (var o in objectlist)
            {
                
                yield return string.Join(separator, properties.Select(p => (p.GetValue(o, null) ?? "").ToString()));
            }
        }
        internal static string SemiColonSplit(IEnumerable<string> list)
        {
            return string.Join(";", list.Select(x => x.ToString()).ToArray());
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
