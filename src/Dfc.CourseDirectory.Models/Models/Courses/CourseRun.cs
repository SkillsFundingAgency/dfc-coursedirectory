
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
        FullTime = 0,
        PartTime = 1,
        Flexible = 2
    }
    public enum AttendancePattern
    {
        [Description("Daytime")]
        Daytime = 1,
        [Description("Evening")]
        Evening = 2,
        [Description("Weekend")]
        Weekend = 3,
        [Description("Day/Block Release")]
        DayOrBlockRelease = 4
    }

    public class CourseRun : ICourseRun // ValueObject<CourseRun>, ICourseRun
    {
        //public Guid id { get; }
        public string CourseDescription { get; set; }
        public string EntryRequirments { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string WhatYoullNeed { get; set; }
        public string WhatYoullNeedToBring { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhereNext { get; set; }
        public string CourseName { get; set; }
        public string ProviderCourseID { get; set; }
        public string DeliveryMode { get; set; }
        public bool FlexibleStartDate { get; set; }
        public DateTime StartDate { get; set; }
        public string CourseURL { get; set; }
        public decimal Cost { get; set; }
        public string CostDescription { get; set; }
        public bool AdvancedLearnerLoan { get; set; }
        public DurationUnit DurationUnit { get; set; }
        public int DurationValue { get; set; }
        public StudyMode StudyMode { get; set; }
        public AttendancePattern AttendancePattern { get; set; }
        public IVenue Venue { get; set; }
        public IProvider Provider { get; set; }
        public IQualification Qualification { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }

        //public CourseRun(
        //    //Guid id,
        //    string coursedescription,
        //    string entryrequirements, //requirements
        //    string whatyoulllearn,
        //    string howyoulllearn,
        //    string whatyoullneed,
        //    string whatyoullneedtobring,
        //    string howyoullbeassessed,
        //    string wherenext,
        //    string coursename,
        //    string providercourseID, //string courseID
        //    string deliverymode,
        //    bool flexiblestartdate,
        //    DateTime startdate,
        //    string courseURL,
        //    decimal cost, //string price,
        //    string costdescription,
        //    bool advancedlearnerloan,
        //    DurationUnit durationunit,
        //    int durationvalue, //string duration,
        //    StudyMode studymode, //string studymode,
        //    AttendancePattern attendancepattern, //string attendance,
        //    //string pattern,
        //    IVenue venue,
        //    IProvider provider,
        //    IQualification qualification,
        //    DateTime createddate,
        //    string createdby,
        //    DateTime updateddate,
        //    string updatedby)
        //{
        //    //Throw.IfNullOrWhiteSpace(id, nameof(id));
        //    Throw.IfNullOrWhiteSpace(coursedescription, nameof(coursedescription));
        //    Throw.IfNullOrWhiteSpace(entryrequirements, nameof(entryrequirements)); 
        //    Throw.IfNullOrWhiteSpace(whatyoulllearn, nameof(whatyoulllearn));
        //    Throw.IfNullOrWhiteSpace(howyoulllearn, nameof(howyoulllearn));
        //    Throw.IfNullOrWhiteSpace(whatyoullneed, nameof(whatyoullneed));
        //    Throw.IfNullOrWhiteSpace(whatyoullneedtobring, nameof(whatyoullneedtobring));
        //    Throw.IfNullOrWhiteSpace(howyoullbeassessed, nameof(howyoullbeassessed));
        //    Throw.IfNullOrWhiteSpace(wherenext, nameof(wherenext));
        //    Throw.IfNullOrWhiteSpace(coursename, nameof(coursename));
        //    Throw.IfNullOrWhiteSpace(providercourseID, nameof(providercourseID));
        //    Throw.IfNullOrWhiteSpace(deliverymode, nameof(deliverymode));
        //    Throw.IfNull(flexiblestartdate, nameof(flexiblestartdate));
        //    Throw.IfNull(startdate, nameof(startdate));
        //    Throw.IfNullOrWhiteSpace(courseURL, nameof(courseURL));
        //    Throw.IfNull(cost, nameof(cost));
        //    Throw.IfLessThan(0, cost, nameof(cost)); //Throw.IfNullOrWhiteSpace(price, nameof(price));
        //    Throw.IfNullOrWhiteSpace(costdescription, nameof(costdescription));
        //    Throw.IfNull(advancedlearnerloan, nameof(advancedlearnerloan));
        //    Throw.IfNull(durationunit, nameof(durationunit));
        //    Throw.IfNull(durationvalue, nameof(durationvalue));
        //    Throw.IfLessThan(1, durationvalue, nameof(durationvalue)); 
        //    Throw.IfNull(studymode, nameof(studymode)); 
        //    Throw.IfNull(attendancepattern, nameof(attendancepattern)); 
        //    Throw.IfNull(venue, nameof(venue));
        //    Throw.IfNull(provider, nameof(provider));
        //    Throw.IfNull(qualification, nameof(qualification));
        //    Throw.IfNull(createddate, nameof(createddate));
        //    Throw.IfNullOrWhiteSpace(createdby, nameof(createdby));
        //    Throw.IfNull(updateddate, nameof(updateddate));
        //    Throw.IfNullOrWhiteSpace(updatedby, nameof(updatedby));

        //    //id = id;
        //    CourseDescription = coursedescription;
        //    EntryRequirments = entryrequirements; //Requirements = requirements;
        //    WhatYoullLearn = whatyoulllearn;
        //    HowYoullLearn =  howyoulllearn;
        //    WhatYoullNeed = whatyoullneed;
        //    WhatYoullNeedToBring = whatyoullneedtobring;
        //    HowYoullBeAssessed = howyoullbeassessed;
        //    WhereNext = wherenext;
        //    CourseName = coursename;
        //    ProviderCourseID = providercourseID; //CourseID = courseID;
        //    DeliveryMode = deliverymode;
        //    FlexibleStartDate = flexiblestartdate;
        //    StartDate = startdate;
        //    CourseURL = courseURL;
        //    Cost = cost; // Price = price;
        //    CostDescription = costdescription;
        //    AdvancedLearnerLoan = advancedlearnerloan;
        //    DurationUnit = durationunit;
        //    DurationValue = durationvalue;
        //    StudyMode = studymode;
        //    AttendancePattern = attendancepattern; //Attendance = attendance;
        //    //Pattern = pattern;
        //    Venue = venue;
        //    Provider = provider;
        //    Qualification = qualification;
        //    CreatedDate = createddate;
        //    CreatedBy = createdby;
        //    UpdatedDate = updateddate;
        //    UpdatedBy = updatedby;
        //}

        //protected override IEnumerable<object> GetEqualityComponents()
        //{
        //    //yield return id;
        //    yield return CourseDescription;
        //    yield return EntryRequirments; // Requirements;
        //    yield return WhatYoullLearn;
        //    yield return HowYoullLearn;
        //    yield return WhatYoullNeed;
        //    yield return WhatYoullNeedToBring;
        //    yield return HowYoullBeAssessed;
        //    yield return WhereNext;
        //    yield return CourseName;
        //    yield return ProviderCourseID; //CourseID
        //    yield return DeliveryMode;
        //    yield return FlexibleStartDate;
        //    yield return StartDate;
        //    yield return CourseURL;
        //    yield return Cost; // Price;
        //    yield return CostDescription;
        //    yield return AdvancedLearnerLoan;
        //    yield return DurationUnit;
        //    yield return DurationValue; // Duration;
        //    yield return StudyMode;
        //    yield return AttendancePattern; //yield return Attendance;
        //    //yield return Pattern;
        //    yield return Venue;
        //    yield return Provider;
        //    yield return Qualification;
        //    yield return CreatedDate;
        //    yield return CreatedBy;
        //    yield return UpdatedDate;
        //    yield return UpdatedBy;
        //}
    }
}