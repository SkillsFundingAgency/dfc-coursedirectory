using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;

namespace Dfc.CourseDirectory.WebV2.Features.ChooseQualification
{
    public class FlowModel : IMptxState
    {
        public string LarsCode { get; set; }
        public string WhoThisCourseIsFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYouWillLearn { get; set; }
        public string HowYouWillLearn { get; set; }
        public string WhatYouWillNeedToBring { get; set; }
        public string HowYouWillBeAssessed { get; set; }
        public string WhereNext { get; set; }
        public CourseDeliveryMode? DeliveryMode { get; set; }
        public string CourseName { get; set; }
        public string ProviderCourseRef { get; set; }
        public DateInput StartDate { get; set; }
        public bool? FlexibleStartDate { get; set; }
        public bool? NationalDelivery { get; set; }
        public IEnumerable<string> SubRegionIds { get; set; }
        public string CourseWebPage { get; set; }
        public string Cost { get; set; }
        public string CostDescription { get; set; }
        public int? Duration { get; set; }
        public CourseDurationUnit? DurationUnit { get; set; }
        public CourseStudyMode? StudyMode { get; set; }
        public CourseAttendancePattern? AttendancePattern { get; set; }
        public Guid? VenueId { get; set; }


        public void SetCourse(string larsCode) => LarsCode = larsCode;

        public void SetCourseDescription(
          string whoThisCourseIsFor,
          string entryRequirements,
          string whatYouWillLearn,
          string howYouWillLearn,
          string whatYouWillNeedToBring,
          string howYouWillBeAssessed,
          string whereNext)
        {
            WhoThisCourseIsFor = whoThisCourseIsFor;
            EntryRequirements = entryRequirements;
            WhatYouWillLearn = whatYouWillLearn;
            HowYouWillLearn = howYouWillLearn;
            WhatYouWillNeedToBring = whatYouWillNeedToBring;
            HowYouWillBeAssessed = howYouWillBeAssessed;
            WhereNext = whereNext;
        }

        public void SetDeliveryMode(CourseDeliveryMode? deliveryMode)
        {
            DeliveryMode = deliveryMode;
        }

        public void SetCourseRun(string courseName,
        string providerCourseRef,
        DateInput startDate,
        bool? flexibleStartDate,
        bool? nationalDelivery,
        IEnumerable<string> subRegionIds,
        string courseWebPage,
        string cost,
        string costDescription,
        int? duration,
        CourseDurationUnit? durationUnit,
        CourseStudyMode? studyMode,
        CourseAttendancePattern? attendancePattern,
       Guid? venueId)
        {
            CourseName = courseName;
            ProviderCourseRef = providerCourseRef;
            StartDate = startDate;
            FlexibleStartDate = flexibleStartDate;
            NationalDelivery = nationalDelivery;
            SubRegionIds = subRegionIds;
            CourseWebPage = courseWebPage;
            Cost = cost;
            CostDescription = costDescription;
            Duration = duration;
            DurationUnit = durationUnit;
            AttendancePattern = attendancePattern;
            StudyMode = studyMode;
            VenueId = venueId;
        }
    }
}
