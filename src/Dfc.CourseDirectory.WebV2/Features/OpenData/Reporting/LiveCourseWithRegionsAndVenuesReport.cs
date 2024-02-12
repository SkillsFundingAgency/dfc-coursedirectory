using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries.OpenData;
using Dfc.CourseDirectory.Core.DataManagement;
using MediatR;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.WebV2.Features.OpenData.Reporting.LiveCoursesWithRegionsAndVenuesReport
{
    public class Query : IRequest<IAsyncEnumerable<Csv>>
    {
        public DateTime FromDate { get; set; }
    }

    public class Csv
    {
        [Name("PROVIDER_UKPRN")]
        public string ProviderUkprn { get; set; }

        [Name("COURSE_ID")]
        public string CourseId { get; set; }

        [Name("COURSE_RUN_ID")]
        public string CourseRunId { get; set; }

        [Name("LEARN_AIM_REF")]
        public string LarsId { get; set; }

        [Name("COURSE_NAME")]
        public string CourseName { get; set; }

        [Name("COURSE_DESCRIPTION")]
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

            static async IAsyncEnumerable<Csv> Process(IAsyncEnumerable<LiveCoursesWithRegionsAndVenuesReportItem> liveCoursesRecords, List<Sector> sectors)
                {
                    await foreach (var record in liveCoursesRecords)
                    {
                        yield return new Csv
                        {
                            ProviderUkprn = record.ProviderUkprn.ToString(),
                            CourseId = record.CourseId.ToString(),
                            CourseRunId = record.CourseRunId.ToString(),
                            CourseName = record.CourseName,
                            CourseDescription = record.CourseDescription,
                            CourseUrl = record.CourseWebsite,
                            LarsId = record.LearnAimRef,
                            DeliveryMode = record.DeliveryMode,
                            AttendancePattern = record.AttendancePattern,
                            StudyMode = record.StudyMode,
                            IsFlexible = record.FlexibleStartDate,
                            StartDate = ParsedCsvCourseRow.MapStartDate(record.StartDate),
                            DurationUnit = record.DurationUnit,
                            DurationValue = record.DurationValue,
                            Cost = ParsedCsvCourseRow.MapCost(record.Cost),
                            CostDescription = record.CostDescription,
                            IsNational = record.National,
                            Regions = record.Regions,
                            LocationName = record.VenueName,
                            LocationAddress1 = record.VenueAddress1,
                            LocationAddress2 = record.VenueAddress2,
                            LocationCounty = record.VenueCounty,
                            LocationEmail = record.VenueEmail,
                            LocationLat = record.VenueLatitude,
                            LocationLon = record.VenueLongitude,
                            LocationPostcode = record.VenuePostcode,
                            LocationTown = record.VenueTown,
                            LocationPhone = record.VenueTelephone,
                            LocationWebsite = record.VenueWebsite,
                            UpdatedDate = ParsedCsvCourseRow.MapStartDate(record.UpdatedOn),
                            EntryRequirements = record.EntryRequirements,
                            HowYouWillBeAssessed = record.HowYouWillBeAssessed,
                            CourseType = record.CourseType,
                            Sector = ParsedCsvNonLarsCourseRow.MapSectorIdToCode(record.SectorId, sectors),
                            AwardingBody = record.AwardingBody,
                            EducationLevel = record.EducationLevel
                        };
                    }
                }
            }
        }
}
