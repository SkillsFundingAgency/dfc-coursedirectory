﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries.OpenData;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.Courses.Reporting.AllCoursesReport
{
    public class Query : IRequest<IAsyncEnumerable<Csv>>
    {
        public DateTime FromDate { get; set; }
    }

    public class Csv
    {
        [Name("PROVIDER_UKPRN")]
        public string ProviderUkprn { get; set; }

        [Name("PROVIDER_NAME")]
        public string ProviderName { get; set; }

        [Name("COURSE_ID")]
        public string CourseId { get; set; }

        [Name("COURSE_RUN_ID")]
        public string CourseRunId { get; set; }

        [Name("LEARN_AIM_REF")]
        public string LarsId { get; set; }

        [Name("COURSE_NAME")]
        public string CourseName { get; set; }

        [Name("WHO_THIS_COURSE_IS_FOR")]
        public string CourseDescription { get; set; }

        [Name("DELIVER_MODE")]
        public int? DeliveryMode { get; set; }

        [Name("STUDY_MODE")]
        public int? StudyMode { get; set; }

        [Name("ATTENDANCE_PATTERN")]
        public int? AttendancePattern { get; set; }

        [Name("FLEXIBLE_STARTDATE")]
        public bool IsFlexible { get; set; }

        [Name("STARTDATE")]
        public string StartDate { get; set; }

        [Name("DURATION_UNIT")]
        public int? DurationUnit { get; set; }

        [Name("DURATION_VALUE")]
        public int? DurationValue { get; set; }

        [Name("COST")]
        public string Cost { get; set; }

        [Name("COST_DESCRIPTION")]
        public string CostDescription { get; set; }

        [Name("NATIONAL")]
        public bool IsNational { get; set; }

        [Name("REGIONS")]
        public string Regions { get; set; }

        [Name("LOCATION_NAME")]
        public string LocationName { get; set; }

        [Name("LOCATION_ADDRESS1")]
        public string LocationAddress1 { get; set; }

        [Name("LOCATION_ADDRESS2")]
        public string LocationAddress2 { get; set; }

        [Name("LOCATION_COUNTY")]
        public string LocationCounty { get; set; }

        [Name("LOCATION_EMAIL")]
        public string LocationEmail { get; set; }

        [Name("LOCATION_LATITUDE")]
        public double? LocationLat { get; set; }

        [Name("LOCATION_LONGITUDE")]
        public double? LocationLon { get; set; }

        [Name("LOCATION_POSTCODE")]
        public string LocationPostcode { get; set; }

        [Name("LOCATION_TELEPHONE")]
        public string LocationPhone { get; set; }

        [Name("LOCATION_TOWN")]
        public string LocationTown { get; set; }

        [Name("LOCATION_WEBSITE")]
        public string LocationWebsite { get; set; }

        [Name("COURSE_URL")]
        public string CourseUrl { get; set; }

        [Name("UPDATED_DATE")]
        public string UpdatedDate { get; set; }

        [Name("ENTRY_REQUIREMENTS")]
        public string EntryRequirements { get; set; }

        [Name("HOW_YOU_WILL_BE_ASSESSED")]
        public string HowYouWillBeAssessed { get; set; }

        [Name("COURSE_TYPE")]
        public int? CourseType { get; set; }

        [Name("SECTOR")]
        public string Sector { get; set; }

        [Name("EDUCATION_LEVEL")]
        public string EducationLevel { get; set; }

        [Name("AWARDING_BODY")]
        public string AwardingBody { get; set; }

        [Name("CREATED_DATE")]
        public string CreatedDate { get; set; }
    }

    public class Handler : IRequestHandler<Query, IAsyncEnumerable<Csv>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<IAsyncEnumerable<Csv>> Handle(Query request, CancellationToken cancellationToken)
        {
            var sectors = (await _sqlQueryDispatcher.ExecuteQuery(new GetSectors())).ToList();

            var liveCoursesRecords = _sqlQueryDispatcher.ExecuteQuery(new GetLiveCoursesWithRegionsAndVenuesReport
            {
                FromDate = request.FromDate
            });

            return Process(liveCoursesRecords, sectors);

            static async IAsyncEnumerable<Csv> Process(IAsyncEnumerable<LiveCoursesWithRegionsAndVenuesReportItem> results, List<Sector> sectors)
            {
                await foreach (var result in results)
                {
                    yield return new Csv
                    {
                        ProviderUkprn = result.ProviderUkprn.ToString(),
                        ProviderName = result.ProviderName,
                        CourseId = result.CourseId.ToString(),
                        CourseRunId = result.CourseRunId.ToString(),
                        CourseName = result.CourseName,
                        CourseDescription = result.CourseDescription,
                        CourseUrl = result.CourseWebsite,
                        LarsId = result.LearnAimRef,
                        DeliveryMode = result.DeliveryMode,
                        AttendancePattern = result.AttendancePattern,
                        StudyMode = result.StudyMode,
                        IsFlexible = result.FlexibleStartDate,
                        StartDate = ParsedCsvCourseRow.MapStartDate(result.StartDate),
                        DurationUnit = result.DurationUnit,
                        DurationValue = result.DurationValue,
                        Cost = ParsedCsvCourseRow.MapCost(result.Cost),
                        CostDescription = result.CostDescription,
                        IsNational = result.National,
                        Regions = result.Regions,
                        LocationName = result.VenueName,
                        LocationAddress1 = result.VenueAddress1,
                        LocationAddress2 = result.VenueAddress2,
                        LocationCounty = result.VenueCounty,
                        LocationEmail = result.VenueEmail,
                        LocationLat = result.VenueLatitude,
                        LocationLon = result.VenueLongitude,
                        LocationPostcode = result.VenuePostcode,
                        LocationTown = result.VenueTown,
                        LocationPhone = result.VenueTelephone,
                        LocationWebsite = result.VenueWebsite,
                        CreatedDate = ParsedCsvCourseRow.MapStartDate(result.CreatedOn),
                        UpdatedDate = ParsedCsvCourseRow.MapStartDate(result.UpdatedOn),
                        EntryRequirements = result.EntryRequirements,
                        HowYouWillBeAssessed = result.HowYouWillBeAssessed,
                        CourseType = result.CourseType,
                        Sector = ParsedCsvNonLarsCourseRow.MapSectorIdToCode(result.SectorId, sectors),
                        EducationLevel = result.EducationLevel,
                        AwardingBody = result.AwardingBody,
                    };
                }
            }
        }
    }
}
