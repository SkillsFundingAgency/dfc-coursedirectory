
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
        [Description("Defined Start Date")]
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

    public enum DeliveryType
    {
        [Description("Classroom")]
        Classroom = 0,
        [Description("Online")]
        Online = 1,
        [Description("Work based")]
        Workbased = 2,
    }

    public class CourseRun : ICourseRun 
    {
        public Guid Id { get; set; }
        public Guid VenueId { get; set; }
        public string CourseName { get; set; }
        public string ProviderCourseID { get; set; }
        public string DeliveryMode { get; set; }
        public bool FlexibleStartDate { get; set; }
        public DateTime StartDate { get; set; }
        public string CourseURL { get; set; }
        public decimal Cost { get; set; }
        public string CostDescription { get; set; }       
        public DurationUnit DurationUnit { get; set; }
        public int DurationValue { get; set; }
        public StudyMode StudyMode { get; set; }
        public AttendancePattern AttendancePattern { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}