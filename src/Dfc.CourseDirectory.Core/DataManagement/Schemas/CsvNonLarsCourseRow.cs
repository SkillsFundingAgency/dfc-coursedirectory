﻿using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataManagement.Schemas
{
    public class CsvNonLarsCourseRow
    {
        public const char SubRegionDelimiter = ';';

        private string _providerCourseRef;
        private string _courseName;
        private string _courseWebPage;

        [Index(0), Name("COURSE_TYPE")]
        public string CourseType { get; set; }
        [Index(1), Name("SECTOR")]
        public string Sector { get; set; }
        [Index(2), Name("EDUCATION_LEVEL")]
        public string EducationLevel { get; set; }
        [Index(3), Name("AWARDING_BODY")]
        public string AwardingBody { get; set; }
        [Index(4), Name("WHO_THIS_COURSE_IS_FOR")]
        public string WhoThisCourseIsFor { get; set; }
        [Index(5), Name("ENTRY_REQUIREMENTS")]
        public string EntryRequirements { get; set; }
        [Index(6), Name("WHAT_YOU_WILL_LEARN")]
        public string WhatYouWillLearn { get; set; }
        [Index(7), Name("HOW_YOU_WILL_LEARN")]
        public string HowYouWillLearn { get; set; }
        [Index(8), Name("WHAT_YOU_WILL_NEED_TO_BRING")]
        public string WhatYouWillNeedToBring { get; set; }
        [Index(9), Name("HOW_YOU_WILL_BE_ASSESSED")]
        public string HowYouWillBeAssessed { get; set; }
        [Index(10), Name("WHERE_NEXT")]
        public string WhereNext { get; set; }
        [Index(11), Name("COURSE_NAME")]
        public string CourseName
        {
            get => string.IsNullOrWhiteSpace(_courseName) ? null : _courseName;
            set => _courseName = value;
        }
        [Index(12), Name("YOUR_REFERENCE")]
        public string ProviderCourseRef
        {
            get => string.IsNullOrWhiteSpace(_providerCourseRef) ? null : _providerCourseRef;
            set => _providerCourseRef = value;
        }
        [Index(13), Name("DELIVERY_MODE")]
        public string DeliveryMode { get; set; }
        [Index(14), Name("START_DATE")]
        public string StartDate { get; set; }
        [Index(15), Name("FLEXIBLE_START_DATE")]
        public string FlexibleStartDate { get; set; }
        [Index(16), Name("VENUE_NAME")]
        public string VenueName { get; set; }
        [Index(17), Name("YOUR_VENUE_REFERENCE")]
        public string ProviderVenueRef { get; set; }
        [Index(18), Name("NATIONAL_DELIVERY")]
        public string NationalDelivery { get; set; }
        [Index(19), Name("SUB_REGION")]
        public string SubRegions { get; set; }
        [Index(20), Name("COURSE_WEBPAGE")]
        public string CourseWebPage
        {
            get => string.IsNullOrWhiteSpace(_courseWebPage) ? null : _courseWebPage;
            set => _courseWebPage = value;
        }
        [Index(21), Name("COST")]
        public string Cost { get; set; }
        [Index(22), Name("COST_DESCRIPTION")]
        public string CostDescription { get; set; }
        [Index(23), Name("DURATION")]
        public string Duration { get; set; }
        [Index(24), Name("DURATION_UNIT")]
        public string DurationUnit { get; set; }
        [Index(25), Name("STUDY_MODE")]
        public string StudyMode { get; set; }
        [Index(26), Name("ATTENDANCE_PATTERN")]
        public string AttendancePattern { get; set; }

        public static CsvNonLarsCourseRow FromModel(CourseUploadRow row) => new CsvNonLarsCourseRow()
        {
            CourseType = row.CourseType,
            Sector = row.Sector,
            AwardingBody = row.AwardingBody,
            EducationLevel  = row.EducationLevel,
            WhoThisCourseIsFor = row.WhoThisCourseIsFor,
            EntryRequirements = row.EntryRequirements,
            WhatYouWillLearn = row.WhatYouWillLearn,
            HowYouWillLearn = row.HowYouWillLearn,
            WhatYouWillNeedToBring = row.WhatYouWillNeedToBring,
            HowYouWillBeAssessed = row.HowYouWillBeAssessed,
            WhereNext = row.WhereNext,
            CourseName = row.CourseName,
            ProviderCourseRef = row.ProviderCourseRef,
            DeliveryMode = row.DeliveryMode,
            StartDate = row.StartDate,
            FlexibleStartDate = row.FlexibleStartDate,
            VenueName = row.VenueName,
            ProviderVenueRef = row.ProviderVenueRef,
            NationalDelivery = row.NationalDelivery,
            SubRegions = row.SubRegions,
            CourseWebPage = row.CourseWebPage,
            Cost = row.Cost,
            CostDescription = row.CostDescription,
            Duration = row.Duration,
            DurationUnit = row.DurationUnit,
            StudyMode = row.StudyMode,
            AttendancePattern = row.AttendancePattern
        };

        public static IEnumerable<CsvNonLarsCourseRow> FromModel(Course row, List<Sector> sectors, IReadOnlyCollection<Region> allRegions) =>
            row.CourseRuns
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.DeliveryMode)
                .Select(courseRun => new CsvNonLarsCourseRow()
                {
                    CourseType = ParsedCsvNonLarsCourseRow.MapCourseType(row.CourseType),
                    Sector = sectors.FirstOrDefault(s => s.Id == row.SectorId)?.Code,                    
                    EducationLevel = ParsedCsvNonLarsCourseRow.MapEducationLevel(row.EducationLevel),
                    AwardingBody = row.AwardingBody,
                    WhoThisCourseIsFor = row.CourseDescription,
                    EntryRequirements = row.EntryRequirements,
                    WhatYouWillLearn = row.WhatYoullLearn,
                    HowYouWillLearn = row.HowYoullLearn,
                    WhatYouWillNeedToBring = row.WhatYoullNeed,
                    HowYouWillBeAssessed = row.HowYoullBeAssessed,
                    WhereNext = row.WhereNext,
                    CourseName = courseRun.CourseName,
                    ProviderCourseRef = courseRun.ProviderCourseId,
                    DeliveryMode = ParsedCsvNonLarsCourseRow.MapDeliveryMode(courseRun.DeliveryMode),
                    StartDate = ParsedCsvNonLarsCourseRow.MapStartDate(courseRun.StartDate),
                    FlexibleStartDate = ParsedCsvNonLarsCourseRow.MapFlexibleStartDate(courseRun.FlexibleStartDate),
                    VenueName = courseRun.VenueName,
                    ProviderVenueRef = courseRun.ProviderVenueRef,
                    NationalDelivery = ParsedCsvNonLarsCourseRow.MapNationalDelivery(courseRun.National),
                    SubRegions = ParsedCsvNonLarsCourseRow.MapSubRegions(courseRun.SubRegionIds, allRegions),
                    CourseWebPage = courseRun.CourseWebsite,
                    Cost = ParsedCsvNonLarsCourseRow.MapCost(courseRun.Cost),
                    CostDescription = courseRun.CostDescription,
                    Duration = ParsedCsvNonLarsCourseRow.MapDuration(courseRun.DurationValue),
                    DurationUnit = ParsedCsvNonLarsCourseRow.MapDurationUnit(courseRun.DurationUnit),
                    StudyMode = ParsedCsvNonLarsCourseRow.MapStudyMode(courseRun.StudyMode) ?? "",
                    AttendancePattern = ParsedCsvNonLarsCourseRow.MapAttendancePattern(courseRun.AttendancePattern) ?? ""
                });

        public static CsvNonLarsCourseRow[][] GroupRows(IEnumerable<CsvNonLarsCourseRow> rows) =>
            rows.GroupBy(r => r, new CsvNonLarsCourseRowCourseComparer())
                .Select(g => g.ToArray())
                .ToArray();

        private class CsvNonLarsCourseRowCourseComparer : IEqualityComparer<CsvNonLarsCourseRow>
        {
            public bool Equals(CsvNonLarsCourseRow x, CsvNonLarsCourseRow y)
            {
                if (x is null && y is null)
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(x.CourseType) || string.IsNullOrEmpty(y.CourseType))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(x.AwardingBody) || string.IsNullOrEmpty(y.AwardingBody))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(x.EducationLevel) || string.IsNullOrEmpty(y.EducationLevel))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(x.Sector) || string.IsNullOrEmpty(y.Sector))
                {
                    return false;
                }

                return
                    x.CourseType == y.CourseType &&
                    x.Sector == y.Sector &&
                    x.AwardingBody == y.AwardingBody &&
                    x.EducationLevel == y.EducationLevel &&                    
                    x.WhoThisCourseIsFor == y.WhoThisCourseIsFor &&
                    x.EntryRequirements == y.EntryRequirements &&
                    x.WhatYouWillLearn == y.WhatYouWillLearn &&
                    x.HowYouWillLearn == y.HowYouWillLearn &&
                    x.WhatYouWillNeedToBring == y.WhatYouWillNeedToBring &&
                    x.HowYouWillBeAssessed == y.HowYouWillBeAssessed &&
                    x.WhereNext == y.WhereNext;
            }

            public int GetHashCode(CsvNonLarsCourseRow obj) =>
                HashCode.Combine(
                    obj.CourseType,
                    obj.WhoThisCourseIsFor,
                    obj.EntryRequirements,
                    obj.WhatYouWillLearn,
                    obj.HowYouWillLearn,
                    obj.WhatYouWillNeedToBring,
                    obj.HowYouWillBeAssessed,
                    obj.WhereNext);
        }
    }
}
