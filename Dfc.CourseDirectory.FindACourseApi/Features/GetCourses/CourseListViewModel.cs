using System;
using System.ComponentModel;
namespace Dfc.CourseDirectory.FindACourseApi.Features.GetCourses
{
    public class CourseListViewModel
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
        public string CourseType { get; set; }
        public string SectorDescription { get; set; }
        public string SectorCode { get; set; }
        public string SectorSubjectArea { get; set; }
        public EducationLevel EducationLevel { get; set; }
        public string AwardingBody { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
        public bool FlexibleStartDate { get; set; }
        public DateTime? StartDate { get; set; }
        public string CourseWebsite { get; set; }
        public decimal? Cost { get; set; }
        public string CostDescription { get; set; }
        public DurationUnit DurationUnit { get; set; }
        public int? DurationValue { get; set; }
        public StudyMode StudyMode { get; set; }
        public AttendancePattern AttendancePattern { get; set; }
        public bool? National { get; set; }
        public string Region { get; set; }
        public string ParentRegion { get; set; }
        public string WhoTheCourseIsFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string WhatYoullNeed { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhatYouCanDoNext { get; set; }
        public string ProviderName { get; set; }
        public string ProviderWebsite { get; set; }
        public string ProviderEmail { get; set; }
        public string ProviderPhoneNumber { get; set; }
        public string VenueName { get; set; }
        public string Postcode { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string LearnAimRef { get; set; }
        public string LearnAimRefTitle  { get; set; }
        public string QualificationLevel { get; set; }
        public string AwardingOrganisation  { get; set; }
    }
    public interface IEnumObj
    {
        public int? Value { get; set; }
        public string Description { get; set; }
    }
    public class EducationLevel:IEnumObj
    {
        [Description("Expected Values : 0 - Not Known, 1 - Entry Level, 2 - Level 1, 3 - Level 2, 4 - Level 3, 5 - Level 4, 6 - Level 5, 7 - Level 6, 8 - Level 7, 9 - Level 8")]
        public int? Value { get; set; }
        [Description("Expected Values : Not Known, Entry Level, Level 1, Level 2, Level 3, Level 4, Level 5, Level 6, Level 7, Level 8")]
        public string Description { get; set; }
    }
    public class DeliveryMode : IEnumObj
    {
        [Description("Expected Values : 0 - Not Known, 1 - Online, 2 - In Person, 3 - Blended")]
        public int? Value { get; set; }
        [Description("Expected Values : Not Known, Online, In Person, Blended")]
        public string Description { get; set; }
    }
    public class DurationUnit : IEnumObj
    {
        [Description("Expected Values : 0 - Not Known, 1 - Hours, 2 - Days, 3 - Weeks, 4 - Months, 5 - Years")]
        public int? Value { get; set; }
        [Description("Expected Values : Not Known, Hours, Days, Weeks, Months, Years")]
        public string Description { get; set; }
    }
    public class StudyMode : IEnumObj
    {
        [Description("Expected Values : 0 - Not Known, 1 - Full Time, 2 - Part Time, 3 - Flexible")]
        public int? Value { get; set; }
        [Description("Expected Values : Not Known, Full Time, Part Time, Flexible")]
        public string Description { get; set; }
    }
    public class AttendancePattern : IEnumObj
    {
        [Description("Expected Values : 0 - Not Known, 1 - Daytime, 2 - Evening, 3 - Weekend")]
        public int? Value { get; set; }
        [Description("Expected Values : Not Known, Daytime, Evening, Weekend")]
        public string Description { get; set; }
    }
    public class UpdateType : IEnumObj
    {
        [Description("Expected Values : 0 - Not Known, 1 - Newly Added Course, 2 - Updated Course, 3 - Deleted Course")]
        public int? Value { get; set; }
        [Description("Expected Values : Not Known, Newly Added Course, Updated Course, Deleted Course")]
        public string Description { get; set; }
    }
    public class CourseRunStatus : IEnumObj
    {
        [Description("Expected Values : 0 - Not Known, 1 - Active, 2 - Inactive, 3 - Completed")]
        public int? Value { get; set; }
        [Description("Expected Values : Not Known, Active, Inactive, Completed")]
        public string Description { get; set; }
    }
}
