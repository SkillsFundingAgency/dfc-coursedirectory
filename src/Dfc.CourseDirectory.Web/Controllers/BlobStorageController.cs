
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
using Dfc.CourseDirectory.Web.Helpers;

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
                                .Where((y => (int)y.CourseStatus == (int)RecordStatus.Live));

            var csvCourses = CoursesToCsvCourses(courses);
            
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
                var headers = from prop in properties
                              from attr in prop.CustomAttributes
                              from custAttr in attr.NamedArguments
                              select custAttr.TypedValue.Value;

                yield return String.Join(separator, headers);
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

        internal IEnumerable<CsvCourse> CoursesToCsvCourses (IEnumerable<Course> courses)
        {
            List<CsvCourse> csvCourses = new List<CsvCourse>();

            foreach (var course in courses)
            {
                //First course run is on same line as course line
                var firstCourseRun = course.CourseRuns.First();

                //Sanitise regions
                if (firstCourseRun.Regions != null)
                    firstCourseRun.Regions = SanitiseRegions(firstCourseRun.Regions);

                SelectRegionModel selectRegionModel = new SelectRegionModel();

                CsvCourse csvCourse = new CsvCourse
                {
                    LearnAimRef = course.LearnAimRef != null ? SanitiseText(course.LearnAimRef) : String.Empty,
                    CourseDescription = course.CourseDescription != null ? SanitiseText(course.CourseDescription): String.Empty,
                    EntryRequirements = course.EntryRequirements != null ? SanitiseText(course.EntryRequirements) : String.Empty,
                    WhatYoullLearn = course.WhatYoullLearn != null ? SanitiseText(course.WhatYoullLearn) : String.Empty,
                    HowYoullLearn = course.HowYoullLearn != null ? SanitiseText(course.HowYoullLearn) : String.Empty,
                    WhatYoullNeed = course.WhatYoullNeed != null ? SanitiseText(course.WhatYoullNeed) : String.Empty,
                    HowYoullBeAssessed = course.HowYoullBeAssessed != null ? SanitiseText(course.HowYoullBeAssessed) : String.Empty,
                    WhereNext = course.WhereNext != null ? SanitiseText(course.WhereNext) : String.Empty,
                    AdvancedLearnerLoan = course.AdvancedLearnerLoan ? "Yes" : "No",
                    AdultEducationBudget = course.AdultEducationBudget ? "Yes" : "No",
                    CourseName = firstCourseRun.CourseName != null ? SanitiseText(firstCourseRun.CourseName) : String.Empty,
                    ProviderCourseID = firstCourseRun.ProviderCourseID != null ? SanitiseText(firstCourseRun.ProviderCourseID) : String.Empty,
                    DeliveryMode = firstCourseRun.DeliveryMode.ToDescription(),
                    StartDate = firstCourseRun.StartDate.HasValue ? firstCourseRun.StartDate.Value.ToString("dd/MM/yyyy") : string.Empty,
                    FlexibleStartDate = firstCourseRun.FlexibleStartDate ? "Yes" : string.Empty,
                    VenueName = firstCourseRun.VenueId.HasValue ? _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(firstCourseRun.VenueId.Value.ToString())).Result.Value.VenueName : null,
                    National = firstCourseRun.National.HasValue ? (firstCourseRun.National.Value ? "Yes" : "No") : string.Empty,
                    Regions = firstCourseRun.Regions != null ? SemiColonSplit(
                                                                selectRegionModel.RegionItems
                                                                .Where(x => firstCourseRun.Regions.Contains(x.Id))
                                                                .Select(y => y.RegionName).ToList())
                                                                : null,
                    SubRegions = firstCourseRun.SubRegions != null ? SemiColonSplit(
                                                                    selectRegionModel.RegionItems.SelectMany(
                                                                        x => x.SubRegion.Where(
                                                                            y => firstCourseRun.Regions.Contains(y.Id)).Select(
                                                                                z => z.SubRegionName).ToList())) : null,
                    CourseURL = firstCourseRun.CourseURL != null ? SanitiseText(firstCourseRun.CourseURL) : String.Empty,
                    Cost = firstCourseRun.Cost,
                    CostDescription = firstCourseRun.CostDescription != null ? SanitiseText(firstCourseRun.CostDescription) : String.Empty,
                    DurationValue = firstCourseRun.DurationValue,
                    DurationUnit = firstCourseRun.DurationUnit.ToDescription(),
                    StudyMode = firstCourseRun.StudyMode.ToDescription(),
                    AttendancePattern = firstCourseRun.AttendancePattern.ToDescription()
                };
                csvCourses.Add(csvCourse);
                foreach (var courseRun in course.CourseRuns)
                {
                    //Ignore the first course run as we've already captured it
                    if (courseRun.id == firstCourseRun.id)
                    {
                        continue;
                    }

                    //Sanitise regions
                    if(courseRun.Regions != null)
                        courseRun.Regions = SanitiseRegions(courseRun.Regions);

                    CsvCourse csvCourseRun = new CsvCourse
                    {
                        LearnAimRef = course.LearnAimRef != null ? SanitiseText(course.LearnAimRef) : String.Empty,
                        CourseName = courseRun.CourseName != null ? SanitiseText(courseRun.CourseName) : String.Empty,
                        ProviderCourseID = courseRun.ProviderCourseID != null ? SanitiseText(courseRun.ProviderCourseID) : String.Empty,
                        DeliveryMode = courseRun.DeliveryMode.ToDescription(),
                        StartDate = courseRun.StartDate.HasValue? courseRun.StartDate.Value.ToString("dd/MM/yyyy") : string.Empty,
                        FlexibleStartDate = courseRun.FlexibleStartDate ? "Yes" : string.Empty,
                        VenueName = courseRun.VenueId.HasValue ? _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(courseRun.VenueId.Value.ToString())).Result.Value.VenueName : null,
                        National = courseRun.National.HasValue ? (firstCourseRun.National.Value ? "Yes" : "No") : string.Empty,
                        Regions = courseRun.Regions != null ? SemiColonSplit(
                                                                selectRegionModel.RegionItems
                                                                .Where(x => courseRun.Regions.Contains(x.Id))
                                                                .Select(y => y.RegionName).ToList())
                                                                : null,
                        SubRegions = courseRun.SubRegions != null ? SemiColonSplit(
                                                                    selectRegionModel.RegionItems.SelectMany(
                                                                        x => x.SubRegion.Where(
                                                                            y => courseRun.Regions.Contains(y.Id)).Select(
                                                                                z => z.SubRegionName).ToList())) : null,
                        CourseURL = courseRun.CourseURL != null ? SanitiseText(courseRun.CourseURL) : String.Empty,
                        Cost = courseRun.Cost,
                        CostDescription = courseRun.CostDescription != null? SanitiseText(courseRun.CostDescription) : String.Empty,
                        DurationValue = courseRun.DurationValue,
                        DurationUnit = courseRun.DurationUnit.ToDescription(),
                        StudyMode = courseRun.StudyMode.ToDescription(),
                        AttendancePattern = courseRun.AttendancePattern.ToDescription()
                    };
                    csvCourses.Add(csvCourseRun);
                }
            }
            return csvCourses;
        }
        
        internal string SanitiseText(string text)
        {
            if (text.Contains(","))
            {
                text = "\"" + text + "\"";
            }
            text = Regex.Replace(text, @"\t|\n|\r", "");
            return text;
        }
        internal IEnumerable<string> SanitiseRegions(IEnumerable<string> regions)
        {
            SelectRegionModel selectRegionModel = new SelectRegionModel();

            foreach (var selectRegionRegionItem in selectRegionModel.RegionItems.OrderBy(x => x.RegionName))
            {
                //If Region is returned, check for existence of any subregions
                if (regions.Contains(selectRegionRegionItem.Id))
                {
                    var subregionsInList = from subRegion in selectRegionRegionItem.SubRegion
                                           where regions.Contains(subRegion.Id)
                                           select subRegion;

                    //If true, then ignore subregions
                    if (subregionsInList.Count() > 0)
                    {
                        foreach (var subRegion in subregionsInList)
                        {
                            regions = regions.Where(x => (x != subRegion.Id)).ToList();

                        }
                    }
                }
            }
            return regions;
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
