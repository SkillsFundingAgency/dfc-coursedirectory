﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Services.Models.Regions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class CourseProvisionHelper : ICourseProvisionHelper
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICourseService _courseService;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ICSVHelper _CSVHelper;

        private ISession _session => _contextAccessor.HttpContext.Session;

        public CourseProvisionHelper(
            IHttpContextAccessor contextAccessor,
            ICourseService courseService,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ICSVHelper CSVHelper)
        {
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher ?? throw new ArgumentNullException(nameof(cosmosDbQueryDispatcher));
            _CSVHelper = CSVHelper ?? throw new ArgumentNullException(nameof(CSVHelper));
        }

        public async Task<FileStreamResult> DownloadCurrentCourseProvisions()
        {
            var UKPRN = _session.GetInt32("UKPRN");
            if (!UKPRN.HasValue)
            {
                return null;
            }

            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderByUkprn { Ukprn = UKPRN.Value });

            var getCoursesResult = await _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN));

            var courses = getCoursesResult
                .Value
                .Value
                .SelectMany(o => o.Value)
                .SelectMany(i => i.Value)
                .Where(c => c.CourseStatus.HasFlag(RecordStatus.Live));

            var csvCourses = CoursesToCsvCourses(courses);

            return CsvCoursesToFileStream(csvCourses, provider?.ProviderName.Replace(" ", ""));
        }

        private IEnumerable<CsvCourse> CoursesToCsvCourses(IEnumerable<Course> courses)
        {
            List<CsvCourse> csvCourses = new List<CsvCourse>();

            foreach (var course in courses)
            {
                //First course run is on same line as course line in CSV
                var courseRuns = course.CourseRuns.Where(r => r.RecordStatus == RecordStatus.Live).ToArray();

                if (!courseRuns.Any())
                {
                    continue;
                }

                var firstCourseRun = courseRuns.First();

                if (firstCourseRun.Regions != null)
                    firstCourseRun.Regions = _CSVHelper.SanitiseRegionTextForCSVOutput(firstCourseRun.Regions);

                SelectRegionModel selectRegionModel = new SelectRegionModel();

                CsvCourse csvCourse = new CsvCourse
                {
                    LearnAimRef = course.LearnAimRef != null ? _CSVHelper.SanitiseTextForCSVOutput(course.LearnAimRef) : string.Empty,
                    CourseDescription = !string.IsNullOrWhiteSpace(course.CourseDescription) ? _CSVHelper.SanitiseTextForCSVOutput(course.CourseDescription) : string.Empty,
                    EntryRequirements = !string.IsNullOrWhiteSpace(course.EntryRequirements) ? _CSVHelper.SanitiseTextForCSVOutput(course.EntryRequirements) : string.Empty,
                    WhatYoullLearn = !string.IsNullOrWhiteSpace(course.WhatYoullLearn) ? _CSVHelper.SanitiseTextForCSVOutput(course.WhatYoullLearn) : string.Empty,
                    HowYoullLearn = !string.IsNullOrWhiteSpace(course.HowYoullLearn) ? _CSVHelper.SanitiseTextForCSVOutput(course.HowYoullLearn) : string.Empty,
                    WhatYoullNeed = !string.IsNullOrWhiteSpace(course.WhatYoullNeed) ? _CSVHelper.SanitiseTextForCSVOutput(course.WhatYoullNeed) : string.Empty,
                    HowYoullBeAssessed = !string.IsNullOrWhiteSpace(course.HowYoullBeAssessed) ? _CSVHelper.SanitiseTextForCSVOutput(course.HowYoullBeAssessed) : string.Empty,
                    WhereNext = !string.IsNullOrWhiteSpace(course.WhereNext) ? _CSVHelper.SanitiseTextForCSVOutput(course.WhereNext) : string.Empty,
                    AdvancedLearnerLoan = course.AdvancedLearnerLoan ? "Yes" : "No",
                    AdultEducationBudget = course.AdultEducationBudget ? "Yes" : "No",
                    CourseName = firstCourseRun.CourseName != null ? _CSVHelper.SanitiseTextForCSVOutput(firstCourseRun.CourseName) : string.Empty,
                    ProviderCourseID = firstCourseRun.ProviderCourseID != null ? _CSVHelper.SanitiseTextForCSVOutput(firstCourseRun.ProviderCourseID) : string.Empty,
                    DeliveryMode = firstCourseRun.DeliveryMode.ToDescription(),
                    StartDate = firstCourseRun.StartDate.HasValue ? firstCourseRun.StartDate.Value.ToString("dd/MM/yyyy") : string.Empty,
                    FlexibleStartDate = firstCourseRun.FlexibleStartDate ? "Yes" : string.Empty,
                    National = firstCourseRun.National.HasValue ? (firstCourseRun.National.Value ? "Yes" : "No") : string.Empty,
                    Regions = firstCourseRun.Regions != null ? _CSVHelper.SemiColonSplit(
                                                                selectRegionModel.RegionItems
                                                                .Where(x => firstCourseRun.Regions.Contains(x.Id))
                                                                .Select(y => _CSVHelper.SanitiseTextForCSVOutput(y.RegionName).Replace(",","")).ToList())
                                                                : string.Empty,
                    SubRegions = firstCourseRun.Regions != null ? _CSVHelper.SemiColonSplit(
                                                                    selectRegionModel.RegionItems.SelectMany(
                                                                        x => x.SubRegion.Where(
                                                                            y => firstCourseRun.Regions.Contains(y.Id)).Select(
                                                                                z => _CSVHelper.SanitiseTextForCSVOutput(z.SubRegionName).Replace(",","")).ToList())) : string.Empty,
                    CourseURL = firstCourseRun.CourseURL != null ? _CSVHelper.SanitiseTextForCSVOutput(firstCourseRun.CourseURL) : string.Empty,
                    Cost = firstCourseRun.Cost.HasValue ? firstCourseRun.Cost.Value.ToString() : string.Empty,
                    CostDescription = firstCourseRun.CostDescription != null ? _CSVHelper.SanitiseTextForCSVOutput(firstCourseRun.CostDescription) : string.Empty,
                    DurationValue = firstCourseRun.DurationValue.HasValue ? firstCourseRun.DurationValue.Value.ToString() : string.Empty,
                    DurationUnit = firstCourseRun.DurationUnit.ToDescription(),
                    StudyMode = firstCourseRun.StudyMode.ToDescription(),
                    AttendancePattern = firstCourseRun.AttendancePattern.ToDescription()
                };
                if(firstCourseRun.VenueId.HasValue)
                {
                    var result = _cosmosDbQueryDispatcher.ExecuteQuery(
                        new GetVenueById() { VenueId = firstCourseRun.VenueId.Value }).Result;
                    
                    if (!string.IsNullOrWhiteSpace(result?.VenueName))
                    {
                        csvCourse.VenueName = result.VenueName;
                    }
                }
                csvCourses.Add(csvCourse);
                foreach (var courseRun in courseRuns)
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
                        LearnAimRef = course.LearnAimRef != null ? _CSVHelper.SanitiseTextForCSVOutput(course.LearnAimRef) : string.Empty,
                        CourseName = courseRun.CourseName != null ? _CSVHelper.SanitiseTextForCSVOutput(courseRun.CourseName) : string.Empty,
                        ProviderCourseID = courseRun.ProviderCourseID != null ? _CSVHelper.SanitiseTextForCSVOutput(courseRun.ProviderCourseID) : string.Empty,
                        DeliveryMode = courseRun.DeliveryMode.ToDescription(),
                        StartDate = courseRun.StartDate.HasValue ? courseRun.StartDate.Value.ToString("dd/MM/yyyy") : string.Empty,
                        FlexibleStartDate = courseRun.FlexibleStartDate ? "Yes" : string.Empty,
                        National = courseRun.National.HasValue ? (courseRun.National.Value ? "Yes" : "No") : string.Empty,
                        Regions = courseRun.Regions != null ? _CSVHelper.SemiColonSplit(
                                                                selectRegionModel.RegionItems
                                                                .Where(x => courseRun.Regions.Contains(x.Id))
                                                                .Select(y => _CSVHelper.SanitiseTextForCSVOutput(y.RegionName).Replace(",","")).ToList())
                                                                : string.Empty,
                        SubRegions = courseRun.Regions != null ? _CSVHelper.SemiColonSplit(
                                                                    selectRegionModel.RegionItems.SelectMany(
                                                                        x => x.SubRegion.Where(
                                                                            y => courseRun.Regions.Contains(y.Id)).Select(
                                                                                z => _CSVHelper.SanitiseTextForCSVOutput(z.SubRegionName).Replace(",","")).ToList())) : string.Empty,
                        CourseURL = courseRun.CourseURL != null ? _CSVHelper.SanitiseTextForCSVOutput(courseRun.CourseURL) : string.Empty,
                        Cost = courseRun.Cost.HasValue ? courseRun.Cost.Value.ToString() : string.Empty,
                        CostDescription = courseRun.CostDescription != null ? _CSVHelper.SanitiseTextForCSVOutput(courseRun.CostDescription) : string.Empty,
                        DurationValue = courseRun.DurationValue.HasValue ? courseRun.DurationValue.Value.ToString() : string.Empty,
                        DurationUnit = courseRun.DurationUnit.ToDescription(),
                        StudyMode = courseRun.StudyMode.ToDescription(),
                        AttendancePattern = courseRun.AttendancePattern.ToDescription()
                    };

                    if (courseRun.VenueId.HasValue)
                    {
                        var result = _cosmosDbQueryDispatcher.ExecuteQuery(new GetVenueById() { VenueId = courseRun.VenueId.Value }).Result;

                        if (!string.IsNullOrWhiteSpace(result?.VenueName))
                        {
                            csvCourseRun.VenueName = result.VenueName;
                        }
                    }
                    csvCourses.Add(csvCourseRun);
                }
            }
            return csvCourses;
        }

        private FileStreamResult CsvCoursesToFileStream(IEnumerable<CsvCourse> csvCourses, string providerName)
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
            result.FileDownloadName = $"{providerName}_Courses_{d.Day:00}_{d.Month:00}_{d.Year}_{d.Hour:00}_{d.Minute:00}.csv";
            return result;
        }
    }
}
