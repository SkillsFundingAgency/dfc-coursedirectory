using System;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Validation;

namespace Dfc.CourseDirectory.Core.DataManagement.Schemas
{
    public class CsvCourseRowWithErrors : CsvCourseRow
    {
        [Index(int.MaxValue), Name("ERRORS")]
        public string Errors { get; set; }

        public new static CsvCourseRowWithErrors FromModel(CourseUploadRow row) => new CsvCourseRowWithErrors()
        {
            LearnAimRef = row.LarsQan,
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
            AttendancePattern = row.AttendancePattern,
            Errors = string.Join(
                "\n",
                row.Errors.Select(errorCode => ErrorRegistry.All[errorCode].GetMessage()))
        };
    }
}
