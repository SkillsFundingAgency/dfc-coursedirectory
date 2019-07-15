using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Regions;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class CourseProvisionHelper : ICourseProvisionHelper
    {
        private readonly ILogger<CourseProvisionHelper> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICourseService _courseService;
        private readonly IVenueService _venueService;
        private readonly IProviderService _providerService;

        private ICSVHelper _CSVHelper;
        private ISession _session => _contextAccessor.HttpContext.Session;
        public CourseProvisionHelper(
            ILogger<CourseProvisionHelper> logger,
                IHttpContextAccessor contextAccessor,
                ICourseService courseService,
                IVenueService venueService,
                IProviderService providerService,
                ICSVHelper CSVHelper)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(venueService, nameof(venueService));
            Throw.IfNull(providerService, nameof(providerService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            _venueService = venueService;
            _providerService = providerService;
            _CSVHelper = CSVHelper;
        }
        public FileStreamResult DownloadCurrentCourseProvisions()
        {
            int? UKPRN;
            IProviderSearchResult providerSearchResult = null;
            string providerName = String.Empty;
            if (_session.GetInt32("UKPRN").HasValue)
            {

                UKPRN = _session.GetInt32("UKPRN").Value;
                providerSearchResult = _providerService.GetProviderByPRNAsync(new Services.ProviderService.ProviderSearchCriteria(UKPRN.Value.ToString())).Result.Value;
                providerName = providerSearchResult.Value.FirstOrDefault()?.ProviderName.Replace(" ", "");
            }
            else
            {
                return null;
            }

            IEnumerable<Course> courses = _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
                                .Result
                                .Value
                                .Value
                                .SelectMany(o => o.Value)
                                .SelectMany(i => i.Value)
                                .Where((y => (int)y.CourseStatus == (int)RecordStatus.Live));

            var csvCourses = CoursesToCsvCourses(courses);

            return CsvCoursesToFileStream(csvCourses, providerName);
        }

        internal IEnumerable<CsvCourse> CoursesToCsvCourses(IEnumerable<Course> courses)
        {
            List<CsvCourse> csvCourses = new List<CsvCourse>();

            foreach (var course in courses)
            {
                //First course run is on same line as course line
                var firstCourseRun = course.CourseRuns.First();

                //Sanitise regions
                if (firstCourseRun.Regions != null)
                    firstCourseRun.Regions = _CSVHelper.SanitiseRegionTextForCSVOutput(firstCourseRun.Regions);

                SelectRegionModel selectRegionModel = new SelectRegionModel();

                CsvCourse csvCourse = new CsvCourse
                {
                    LearnAimRef = course.LearnAimRef != null ? _CSVHelper.SanitiseTextForCSVOutput(course.LearnAimRef) : String.Empty,
                    CourseDescription = course.CourseDescription != null ? _CSVHelper.SanitiseTextForCSVOutput(course.CourseDescription) : String.Empty,
                    EntryRequirements = course.EntryRequirements != null ? _CSVHelper.SanitiseTextForCSVOutput(course.EntryRequirements) : String.Empty,
                    WhatYoullLearn = course.WhatYoullLearn != null ? _CSVHelper.SanitiseTextForCSVOutput(course.WhatYoullLearn) : String.Empty,
                    HowYoullLearn = course.HowYoullLearn != null ? _CSVHelper.SanitiseTextForCSVOutput(course.HowYoullLearn) : String.Empty,
                    WhatYoullNeed = course.WhatYoullNeed != null ? _CSVHelper.SanitiseTextForCSVOutput(course.WhatYoullNeed) : String.Empty,
                    HowYoullBeAssessed = course.HowYoullBeAssessed != null ? _CSVHelper.SanitiseTextForCSVOutput(course.HowYoullBeAssessed) : String.Empty,
                    WhereNext = course.WhereNext != null ? _CSVHelper.SanitiseTextForCSVOutput(course.WhereNext) : String.Empty,
                    AdvancedLearnerLoan = course.AdvancedLearnerLoan ? "Yes" : "No",
                    AdultEducationBudget = course.AdultEducationBudget ? "Yes" : "No",
                    CourseName = firstCourseRun.CourseName != null ? _CSVHelper.SanitiseTextForCSVOutput(firstCourseRun.CourseName) : String.Empty,
                    ProviderCourseID = firstCourseRun.ProviderCourseID != null ? _CSVHelper.SanitiseTextForCSVOutput(firstCourseRun.ProviderCourseID) : String.Empty,
                    DeliveryMode = firstCourseRun.DeliveryMode.ToDescription(),
                    StartDate = firstCourseRun.StartDate.HasValue ? firstCourseRun.StartDate.Value.ToString("dd/MM/yyyy") : string.Empty,
                    FlexibleStartDate = firstCourseRun.FlexibleStartDate ? "Yes" : string.Empty,
                    VenueName = firstCourseRun.VenueId.HasValue ? _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(firstCourseRun.VenueId.Value.ToString())).Result.Value.VenueName : null,
                    National = firstCourseRun.National.HasValue ? (firstCourseRun.National.Value ? "Yes" : "No") : string.Empty,
                    Regions = firstCourseRun.Regions != null ? _CSVHelper.SemiColonSplit(
                                                                selectRegionModel.RegionItems
                                                                .Where(x => firstCourseRun.Regions.Contains(x.Id))
                                                                .Select(y => _CSVHelper.SanitiseTextForCSVOutput(y.RegionName).Replace(",","")).ToList())
                                                                : null,
                    SubRegions = firstCourseRun.SubRegions != null ? _CSVHelper.SemiColonSplit(
                                                                    selectRegionModel.RegionItems.SelectMany(
                                                                        x => x.SubRegion.Where(
                                                                            y => firstCourseRun.Regions.Contains(y.Id)).Select(
                                                                                z => _CSVHelper.SanitiseTextForCSVOutput(z.SubRegionName).Replace(",","")).ToList())) : null,
                    CourseURL = firstCourseRun.CourseURL != null ? _CSVHelper.SanitiseTextForCSVOutput(firstCourseRun.CourseURL) : String.Empty,
                    Cost = firstCourseRun.Cost,
                    CostDescription = firstCourseRun.CostDescription != null ? _CSVHelper.SanitiseTextForCSVOutput(firstCourseRun.CostDescription) : String.Empty,
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
                    if (courseRun.Regions != null)
                        courseRun.Regions = _CSVHelper.SanitiseRegionTextForCSVOutput(courseRun.Regions);

                    CsvCourse csvCourseRun = new CsvCourse
                    {
                        LearnAimRef = course.LearnAimRef != null ? _CSVHelper.SanitiseTextForCSVOutput(course.LearnAimRef) : String.Empty,
                        CourseName = courseRun.CourseName != null ? _CSVHelper.SanitiseTextForCSVOutput(courseRun.CourseName) : String.Empty,
                        ProviderCourseID = courseRun.ProviderCourseID != null ? _CSVHelper.SanitiseTextForCSVOutput(courseRun.ProviderCourseID) : String.Empty,
                        DeliveryMode = courseRun.DeliveryMode.ToDescription(),
                        StartDate = courseRun.StartDate.HasValue ? courseRun.StartDate.Value.ToString("dd/MM/yyyy") : string.Empty,
                        FlexibleStartDate = courseRun.FlexibleStartDate ? "Yes" : string.Empty,
                        VenueName = courseRun.VenueId.HasValue ? _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(courseRun.VenueId.Value.ToString())).Result.Value.VenueName : null,
                        National = courseRun.National.HasValue ? (firstCourseRun.National.Value ? "Yes" : "No") : string.Empty,
                        Regions = courseRun.Regions != null ? _CSVHelper.SemiColonSplit(
                                                                selectRegionModel.RegionItems
                                                                .Where(x => courseRun.Regions.Contains(x.Id))
                                                                .Select(y => _CSVHelper.SanitiseTextForCSVOutput(y.RegionName).Replace(",","")).ToList())
                                                                : null,
                        SubRegions = courseRun.SubRegions != null ? _CSVHelper.SemiColonSplit(
                                                                    selectRegionModel.RegionItems.SelectMany(
                                                                        x => x.SubRegion.Where(
                                                                            y => courseRun.Regions.Contains(y.Id)).Select(
                                                                                z => _CSVHelper.SanitiseTextForCSVOutput(z.SubRegionName).Replace(",","")).ToList())) : null,
                        CourseURL = courseRun.CourseURL != null ? _CSVHelper.SanitiseTextForCSVOutput(courseRun.CourseURL) : String.Empty,
                        Cost = courseRun.Cost,
                        CostDescription = courseRun.CostDescription != null ? _CSVHelper.SanitiseTextForCSVOutput(courseRun.CostDescription) : String.Empty,
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

        internal FileStreamResult CsvCoursesToFileStream(IEnumerable<CsvCourse> csvCourses, string providerName)
        {
            List<string> csvLines = new List<string>();
            foreach (var line in _CSVHelper.ToCsv(csvCourses))
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

    }
    internal static class TwoCharsClass
    {
        internal static string TwoChars(this int extendee)
        {
            return extendee.ToString().Length < 2 ? $"0{extendee.ToString()}" : extendee.ToString();
        }
    }
}
