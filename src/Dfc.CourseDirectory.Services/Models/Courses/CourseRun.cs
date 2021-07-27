using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Services.Models.Courses
{
    public enum AttendancePatternMonToFri
    {
        [Display(Name = "Undefined")]
        [Description("Undefined")]
        Undefined = 0,
        [Display(Name = "Monday to Friday")]
        [Description("Monday To Friday")]
        MondayToFriday = 1
    }

    public enum StartDateType
    {
        [Display(Name = "Defined Start Date")]
        [Description("Defined Start Date")]
        SpecifiedStartDate = 1,
        [Display(Name = "Flexible Start Date")]
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
        //[Description("Bulk Upload Course")]
        //BulkUploadCourse = 6,
        [Description("Migrate Course")]
        MigrateCourse = 7,
        [Description("DQI")]
        DataQualityIndicator = 8
    }
}
