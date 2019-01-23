using System;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public enum DeliveryMode
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Classroom based")]
        ClassroomBased = 1,
        [Description("Online")]
        Online = 2,
        [Description("Work based")]
        WorkBased = 3
    }
    public enum DurationUnit
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Days")]
        Days = 1,
        [Description("Weeks")]
        Weeks = 2,
        [Description("Months")]
        Months = 3,
        [Description("Years")]
        Years = 4
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

    public class CourseRun : ICourseRun 
    {
        public Guid id { get; set; }
        public int? CourseInstanceId { get; set; }
        public Guid? VenueId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter Course Name")]
        [MaxLength(255, ErrorMessage = "The maximum length of Course Name is 255 characters")]
        [RegularExpression(@"[a-zA-Z0-9 \¬\!\£\$\%\^\&\*\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`\" + "\"" + @"\\]+", ErrorMessage = "Course Name contains invalid characters")]
        public string CourseName { get; set; }

        [MaxLength(255, ErrorMessage = "The maximum length of 'ID' is 255 characters")]
        [RegularExpression(@"[a-zA-Z0-9 \¬\!\£\$\%\^\&\*\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`\" + "\"" + @"\\]+", ErrorMessage = "ID contains invalid characters")]
        public string ProviderCourseID { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
        public bool FlexibleStartDate { get; set; }
        public DateTime? StartDate { get; set; }
        public string CourseURL { get; set; }
        public decimal? Cost { get; set; }
        public string CostDescription { get; set; }       
        public DurationUnit DurationUnit { get; set; }
        public int? DurationValue { get; set; }
        public StudyMode StudyMode { get; set; }
        public AttendancePattern AttendancePattern { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}