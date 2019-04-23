using System;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Regions;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public enum DeliveryMode
    {
        [Description("Undefined")]
        Undefined = 0,
        [DisplayName("Classroom based")]
        [Description("Classroom based")]
        ClassroomBased = 1,
        [Description("Online")]
        Online = 2,
        [DisplayName("Work based")]
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
        Years = 4,
        [Description("Hours")]
        Hours = 5
    }
    public enum StudyMode
    {
        [Description("Undefined")]
        Undefined = 0,
        [DisplayName("Full-time")]
        [Description("Full-time")]
        FullTime = 1,
        [DisplayName("Part-time")]
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
        [DisplayName("Day/Block Release")]
        [Description("Day/Block Release")]
        DayOrBlockRelease = 4
    }
    public enum StartDateType
    {
        [DisplayName("Defined Start Date")]
        [Description("Defined Start Date")]
        SpecifiedStartDate = 1,
        [DisplayName("Flexible Start Date")]
        [Description("Select a flexible start date")] 
        FlexibleStartDate = 2,
    }

    public enum ValidationMode
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Add CourseRun")]
        AddCourseRun = 1,
        [Description("Edit Course YC")]
        EditCourseYC = 2,
        [Description("Edit Course BU")]
        EditCourseBU = 3,
        [Description("Edit Course MT")]
        EditCourseMT = 4,
        [Description("Copy CourseRun")]
        CopyCourseRun = 5,
        [Description("Bulk Upload Course")]
        BulkUploadCourse = 6,
        [Description("Migrate Course")]
        MigrateCourse = 7,
        [Description("DQI")]
        DataQualityIndicator = 8
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

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? StartDate { get; set; }
        public string CourseURL { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public decimal? Cost { get; set; }
        public string CostDescription { get; set; }       
        public DurationUnit DurationUnit { get; set; }
        [Required(ErrorMessage = "Enter Duration")]
        [RegularExpression("^([0-9]|[0-9][0-9]|[0-9][0-9][0-9])$", ErrorMessage = "Duration must be numeric and maximum length is 3 digits")]
        public int? DurationValue { get; set; }
        public StudyMode StudyMode { get; set; }
        public AttendancePattern AttendancePattern { get; set; }
        public IEnumerable<string> Regions { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public IEnumerable<SubRegionItemModel> SubRegions { get; set; }
    }
}