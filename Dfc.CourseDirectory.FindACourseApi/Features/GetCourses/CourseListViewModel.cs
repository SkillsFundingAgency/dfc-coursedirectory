using System;
using Swashbuckle.AspNetCore.Annotations;
namespace Dfc.CourseDirectory.FindACourseApi.Features.GetCourses
{
    public class CourseListViewModel
    {
        [SwaggerSchema(Description = "Unique identifier for the course run")]
        public Guid Id { get; set; }
        [SwaggerSchema(Description = "Unique identifier for the course")]
        public Guid CourseId { get; set; }
        [SwaggerSchema(Description = "Name of the course")]
        public string CourseName { get; set; }
        [SwaggerSchema(Description = "Type of the course")]
        public string CourseType { get; set; }
        [SwaggerSchema(Description = "Description of the sector belong to the course")]
        public string SectorDescription { get; set; }
        [SwaggerSchema(Description = "Code of the sector")]
        public string SectorCode { get; set; }
        [SwaggerSchema(Description = "Subject area of the sector belong to the course")]
        public string SectorSubjectArea { get; set; }
        [SwaggerSchema(Description = "Education level of the course")]
        public CourseEducationLevel EducationLevel { get; set; }
        [SwaggerSchema(Description = "Awarding body of the course")]
        public string AwardingBody { get; set; }
        [SwaggerSchema(Description = "Delivery mode of the course")]
        public DeliveryMode DeliveryMode { get; set; }
        [SwaggerSchema(Description = "Indicates if the course has a flexible start date")]
        public bool FlexibleStartDate { get; set; }
        [SwaggerSchema(Description = "Start date of the course")]
        public DateTime? StartDate { get; set; }
        [SwaggerSchema(Description = "Valid Website URL of the course")]
        public string CourseWebsite { get; set; }
        [SwaggerSchema(Description = "Cost of the course")]
        public decimal? Cost { get; set; }
        [SwaggerSchema(Description = "Description of the course cost")]
        public string CostDescription { get; set; }
        [SwaggerSchema(Description = "Unit of the course duration")]
        public DurationUnit DurationUnit { get; set; }
        [SwaggerSchema(Description = "Value of the course duration")]
        public int? DurationValue { get; set; }
        [SwaggerSchema(Description = "Study mode of the course")]
        public StudyMode StudyMode { get; set; }
        [SwaggerSchema(Description = "Attendance pattern of the course")]
        public AttendancePattern AttendancePattern { get; set; }
        [SwaggerSchema(Description = "Indicates if the course is available nationally")]
        public bool? National { get; set; }
        [SwaggerSchema(Description = "Region where the course is available")]   
        public string Region { get; set; }
        [SwaggerSchema(Description = "Parent region where the course is available")]
        public string ParentRegion { get; set; }
        [SwaggerSchema(Description = "Description of who the course is for")]
        public string WhoTheCourseIsFor { get; set; }
        [SwaggerSchema(Description = "Description of the entry requirements for the course")]
        public string EntryRequirements { get; set; }
        [SwaggerSchema(Description = "Description of what you'll learn in the course")]
        public string WhatYoullLearn { get; set; }
        [SwaggerSchema(Description = "Description of how you'll learn in the course")]
        public string HowYoullLearn { get; set; }
        [SwaggerSchema(Description = "Description of what you'll need for the course")]
        public string WhatYoullNeed { get; set; }
        [SwaggerSchema(Description = "Description of how you'll be assessed in the course")]
        public string HowYoullBeAssessed { get; set; }
        [SwaggerSchema(Description = "Description of what you can do after completing the course")]
        public string WhatYouCanDoNext { get; set; }
        [SwaggerSchema(Description = "Description of the course's accessibility")]
        public string ProviderName { get; set; }
        [SwaggerSchema(Description = "Website of the course provider")]
        public string ProviderWebsite { get; set; }
        [SwaggerSchema(Description = "Email of the course provider")]
        public string ProviderEmail { get; set; }
        [SwaggerSchema(Description = "Phone number of the course provider")]
        public string ProviderPhoneNumber { get; set; }
        [SwaggerSchema(Description = "Name of the venue where the course is delivered")]
        public string VenueName { get; set; }
        [SwaggerSchema(Description = "Postcode of the venue where the course is delivered")]
        public string Postcode { get; set; }
        [SwaggerSchema(Description = "Address line 1 of the venue where the course is delivered")]
        public string AddressLine1 { get; set; }
        [SwaggerSchema(Description = "Address line 2 of the venue where the course is delivered")]
        public string AddressLine2 { get; set; }
        [SwaggerSchema(Description = "Town of the venue where the course is delivered")]
        public string Town { get; set; }
        [SwaggerSchema(Description = "County of the venue where the course is delivered")]
        public string County { get; set; }
        [SwaggerSchema(Description = "Latitude of the venue where the course is delivered")]
        public decimal? Latitude { get; set; }
        [SwaggerSchema(Description = "Longitude of the venue where the course is delivered")]
        public decimal? Longitude { get; set; }
        [SwaggerSchema(Description = "Learning Aim Reference (LARs) Code for the course")]
        public string LearnAimRef { get; set; }
        [SwaggerSchema(Description = "Title of the Learning Aim Reference (LARs) for the course")]
        public string LearnAimRefTitle  { get; set; }
        [SwaggerSchema(Description = "Qualification level of the course based on the Learning Aim Reference (LARs)")]
        public string QualificationLevel { get; set; }
        [SwaggerSchema(Description = "Awarding organisation for the course based on the Learning Aim Reference (LARs)")]
        public string AwardingOrganisation  { get; set; }
    }
    public interface IEnumObj
    {
        public int? Value { get; set; }
        public string Description { get; set; }
    }
    public class CourseEducationLevel : IEnumObj
    {
        [SwaggerSchema(Description = "Expected Values : 0 - Entry Level, 1 - Level 1, 2 - Level 2, 3 - Level 3, 4 - Level 4, 5 - Level 5, 6 - Level 6, 7 - Level 7, 8 - Level 8")]
        public int? Value { get; set; }
        [SwaggerSchema(Description = "Expected Values : Entry Level, Level 1, Level 2, Level 3, Level 4, Level 5, Level 6, Level 7, Level 8")]
        public string Description { get; set; }
    }
    public class DeliveryMode : IEnumObj
    {
        [SwaggerSchema(Description = "Expected Values :  1 - ClassroomBased, 2 - Online, 3 - WorkBased, 4 - BlendedLearning")]
        public int? Value { get; set; }
        [SwaggerSchema(Description = "Expected Values : ClassroomBased, Online, WorkBased, BlendedLearning")]
        public string Description { get; set; }
    }
    public class DurationUnit : IEnumObj
    {
        [SwaggerSchema(Description = "Expected Values : 1 - Days, 2 - Weeks, 3 - Months, 4 - Years, 5 - Hours, 6 - Minutes")]
        public int? Value { get; set; }
        [SwaggerSchema(Description = "Expected Values : Days, Weeks, Months, Years, Hours, Minutes")]
        public string Description { get; set; }
    }
    public class StudyMode : IEnumObj
    {
        [SwaggerSchema(Description = "Expected Values : 1 - Full Time, 2 - Part Time, 3 - Flexible")]
        public int? Value { get; set; }
        [SwaggerSchema(Description = "Expected Values : Full Time, Part Time, Flexible")]
        public string Description { get; set; }
    }
    public class AttendancePattern : IEnumObj
    {
        [SwaggerSchema(Description = "Expected Values : 1 - Daytime, 2 - Evening, 3 - Weekend, 4 - DayOrBlockRelease")]
        public int? Value { get; set; }
        [SwaggerSchema(Description = "Expected Values : Daytime, Evening, Weekend, DayOrBlockRelease")]
        public string Description { get; set; }
    }
    public class UpdateType : IEnumObj
    {
        [SwaggerSchema(Description = "Expected Values : 1 - Newly Added Course, 2 - Updated Course, 3 - Deleted Course")]
        public int? Value { get; set; }
        [SwaggerSchema(Description = "Expected Values : Newly Added Course, Updated Course, Deleted Course")]
        public string Description { get; set; }
    }
    public class CourseRunStatus : IEnumObj
    {
        [SwaggerSchema(Description = "Expected Values : 1 - Live, 4 - Archived")]
        public int? Value { get; set; }
        [SwaggerSchema(Description = "Expected Values : Live, Archived")]
        public string Description { get; set; }
    }
}
