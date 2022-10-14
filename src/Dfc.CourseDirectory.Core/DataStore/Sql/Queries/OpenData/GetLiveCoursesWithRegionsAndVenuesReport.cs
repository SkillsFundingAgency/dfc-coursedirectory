using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries.OpenData
{
    public class GetLiveCoursesWithRegionsAndVenuesReport : ISqlQuery<IAsyncEnumerable<LiveCoursesWithRegionsAndVenuesReportItem>>
    {
        public DateTime FromDate { get; set; }
    }

    public class LiveCoursesWithRegionsAndVenuesReportItem
    {
        public int ProviderUkprn { get; set; }
        public Guid CourseRunId { get; set; }
        public Guid CourseId { get; set; }
        public Guid TLevelId { get; set; }

        public string LearnAimRef { get; set; }
        public string CourseName { get; set; }
        public string CourseDescription { get; set; }
        public string CourseWebsite { get; set; }
        public decimal? Cost { get; set; }
        public string CostDescription { get; set; }
        public bool FlexibleStartDate { get; set; }
        public DateTime? StartDate { get; set; }
        public string EntryRequirements { get; set; }
        public string HowYouWillBeAssessed { get; set; }
        public int? DeliveryMode { get; set; }
        public int? AttendancePattern { get; set; }
        public int? StudyMode { get; set; }
        public int? DurationUnit { get; set; }
        public int? DurationValue { get; set; }
        public bool National { get; set; }
        public string Regions { get; set; }
        public string VenueName { get; set; }
        public string VenueAddress1 { get; set; }
        public string VenueAddress2 { get; set; }
        public string VenueCounty { get; set; }
        public string VenuePostcode { get; set; }
        public string VenueTown { get; set; }
        public double? VenueLatitude { get; set; }
        public double? VenueLongitude { get; set; }
        public string VenueTelephone { get; set; }
        public string VenueEmail { get; set; }
        public string VenueWebsite { get; set; }
        public DateTime? UpdatedOn { get; set; }

    }
}
