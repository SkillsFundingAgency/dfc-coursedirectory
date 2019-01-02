
using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Venues;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Models.Models.Qualifications;
using Dfc.CourseDirectory.Models.Interfaces.Venues;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Interfaces.Providers;
using Dfc.CourseDirectory.Models.Interfaces.Qualifications;
using System.ComponentModel;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public enum DurationUnit
    {
        Day = 0,
        Week = 1,
        Month = 2,
        Year = 3
    }
    public enum StudyMode
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Full-time")]
        FullTime = 1,
        [Description("Part-time")]
        PartTime = 2,
        [Description("Flexible")]
        Flexible = 3
    }
    public enum AttendancePattern
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Daytime")]
        Daytime = 1,
        [Description("Evening")]
        Evening = 2,
        [Description("Weekend")]
        Weekend = 3,
        [Description("Day/Block Release")]
        DayOrBlockRelease = 4
    }
    public enum StartDateType
    {
        [Description("Specified")]
        SpecifiedStartDate = 1,
        [Description("Select a flexible start date")]
        FlexibleStartDate = 2,
    }


    public enum Duration
    {
        [Description("Days")]
        Days = 0,
        [Description("Weeks")]
        Weeks = 1,
        [Description("Months")]
        Months = 2,
        [Description("Years")]
        Years = 3,
    }

    public class CourseRun : ValueObject<CourseRun>, ICourseRun
    {
        //public Guid id { get; }
        public string CourseDescription { get; }
        public string EntryRequirments { get; }
        public string WhatYoullLearn { get; }
        public string HowYoullLearn { get; }
        public string WhatYoullNeed { get; }
        public string WhatYoullNeedToBring { get; }
        public string HowYoullBeAssessed { get; }
        public string WhereNext { get; }
        public string CourseName { get; }
        public string ProviderCourseID { get; }
        public string DeliveryMode { get; }
        public bool FlexibleStartDate { get; }
        public DateTime StartDate { get; }
        public string CourseURL { get; }
        public decimal Cost { get; }
        public string CostDescription { get; }
        public bool AdvancedLearnerLoan { get; }
        public DurationUnit DurationUnit { get; }
        public int DurationValue { get; }
        public StudyMode StudyMode { get; }
        public AttendancePattern AttendancePattern { get; }
        public IVenue Venue { get; }
        public IProvider Provider { get; }
        public IQualification Qualification { get; }

        public DateTime CreatedDate { get; }
        public string CreatedBy { get; }
        public DateTime UpdatedDate { get; }
        public string UpdatedBy { get; }

        public CourseRun(
            //Guid id,
            string coursedescription,
            string entryrequirements, //requirements
            string whatyoulllearn,
            string howyoulllearn,
            string whatyoullneed,
            string whatyoullneedtobring,
            string howyoullbeassessed,
            string wherenext,
            string coursename,
            string providercourseID, //string courseID
            string deliverymode,
            bool flexiblestartdate,
            DateTime startdate,
            string courseURL,
            decimal cost, //string price,
            string costdescription,
            bool advancedlearnerloan,
            DurationUnit durationunit,
            int durationvalue, //string duration,
            StudyMode studymode, //string studymode,
            AttendancePattern attendancepattern, //string attendance,
            //string pattern,
            IVenue venue,
            IProvider provider,
            IQualification qualification,
            DateTime createddate,
            string createdby,
            DateTime updateddate,
            string updatedby)
        {
            //Throw.IfNullOrWhiteSpace(id, nameof(id));
            Throw.IfNullOrWhiteSpace(coursedescription, nameof(coursedescription));
            Throw.IfNullOrWhiteSpace(entryrequirements, nameof(entryrequirements)); 
            Throw.IfNullOrWhiteSpace(whatyoulllearn, nameof(whatyoulllearn));
            Throw.IfNullOrWhiteSpace(howyoulllearn, nameof(howyoulllearn));
            Throw.IfNullOrWhiteSpace(whatyoullneed, nameof(whatyoullneed));
            Throw.IfNullOrWhiteSpace(whatyoullneedtobring, nameof(whatyoullneedtobring));
            Throw.IfNullOrWhiteSpace(howyoullbeassessed, nameof(howyoullbeassessed));
            Throw.IfNullOrWhiteSpace(wherenext, nameof(wherenext));
            Throw.IfNullOrWhiteSpace(coursename, nameof(coursename));
            Throw.IfNullOrWhiteSpace(providercourseID, nameof(providercourseID));
            Throw.IfNullOrWhiteSpace(deliverymode, nameof(deliverymode));
            Throw.IfNull(flexiblestartdate, nameof(flexiblestartdate));
            Throw.IfNull(startdate, nameof(startdate));
            Throw.IfNullOrWhiteSpace(courseURL, nameof(courseURL));
            Throw.IfNull(cost, nameof(cost));
            Throw.IfLessThan(0, cost, nameof(cost)); //Throw.IfNullOrWhiteSpace(price, nameof(price));
            Throw.IfNullOrWhiteSpace(costdescription, nameof(costdescription));
            Throw.IfNull(advancedlearnerloan, nameof(advancedlearnerloan));
            Throw.IfNull(durationunit, nameof(durationunit));
            Throw.IfNull(durationvalue, nameof(durationvalue));
            Throw.IfLessThan(1, durationvalue, nameof(durationvalue)); 
            Throw.IfNull(studymode, nameof(studymode)); 
            Throw.IfNull(attendancepattern, nameof(attendancepattern)); 
            Throw.IfNull(venue, nameof(venue));
            Throw.IfNull(provider, nameof(provider));
            Throw.IfNull(qualification, nameof(qualification));
            Throw.IfNull(createddate, nameof(createddate));
            Throw.IfNullOrWhiteSpace(createdby, nameof(createdby));
            Throw.IfNull(updateddate, nameof(updateddate));
            Throw.IfNullOrWhiteSpace(updatedby, nameof(updatedby));

            //id = id;
            CourseDescription = coursedescription;
            EntryRequirments = entryrequirements; //Requirements = requirements;
            WhatYoullLearn = whatyoulllearn;
            HowYoullLearn =  howyoulllearn;
            WhatYoullNeed = whatyoullneed;
            WhatYoullNeedToBring = whatyoullneedtobring;
            HowYoullBeAssessed = howyoullbeassessed;
            WhereNext = wherenext;
            CourseName = coursename;
            ProviderCourseID = providercourseID; //CourseID = courseID;
            DeliveryMode = deliverymode;
            FlexibleStartDate = flexiblestartdate;
            StartDate = startdate;
            CourseURL = courseURL;
            Cost = cost; // Price = price;
            CostDescription = costdescription;
            AdvancedLearnerLoan = advancedlearnerloan;
            DurationUnit = durationunit;
            DurationValue = durationvalue;
            StudyMode = studymode;
            AttendancePattern = attendancepattern; //Attendance = attendance;
            //Pattern = pattern;
            Venue = venue;
            Provider = provider;
            Qualification = qualification;
            CreatedDate = createddate;
            CreatedBy = createdby;
            UpdatedDate = updateddate;
            UpdatedBy = updatedby;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            //yield return id;
            yield return CourseDescription;
            yield return EntryRequirments; // Requirements;
            yield return WhatYoullLearn;
            yield return HowYoullLearn;
            yield return WhatYoullNeed;
            yield return WhatYoullNeedToBring;
            yield return HowYoullBeAssessed;
            yield return WhereNext;
            yield return CourseName;
            yield return ProviderCourseID; //CourseID
            yield return DeliveryMode;
            yield return FlexibleStartDate;
            yield return StartDate;
            yield return CourseURL;
            yield return Cost; // Price;
            yield return CostDescription;
            yield return AdvancedLearnerLoan;
            yield return DurationUnit;
            yield return DurationValue; // Duration;
            yield return StudyMode;
            yield return AttendancePattern; //yield return Attendance;
            //yield return Pattern;
            yield return Venue;
            yield return Provider;
            yield return Qualification;
            yield return CreatedDate;
            yield return CreatedBy;
            yield return UpdatedDate;
            yield return UpdatedBy;
        }
    }
}