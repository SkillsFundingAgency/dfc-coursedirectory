using System;
using Dfc.CourseDirectory.FindACourseApi.Features.GetCourses;
using Swashbuckle.AspNetCore.Annotations;

namespace Dfc.CourseDirectory.FindACourseApi.Features.GetCourseUpdates
{
    public class CourseUpdatesViewModel
    {
        [SwaggerSchema(Description = "The unique identifier of the course run.")]
        public Guid Id { get; set; }
        [SwaggerSchema(Description = "The type of update for the course.")]
        public UpdateType UpdateType { get; set; }
        [SwaggerSchema(Description = "The status of the course run.")]
        public CourseRunStatus CourseRunStatus { get; set; }
        [SwaggerSchema(Description = "The type of contact for the course.")]
        public string ContactType { get; set; }
        [SwaggerSchema(Description = "The date and time when the course run was created.")]
        public DateTime? CreatedOn { get; set; }
        [SwaggerSchema(Description = "The date and time when the course run was last updated.")]
        public DateTime? UpdatedOn { get; set; }
        [SwaggerSchema(Description = "The date and time when the course was last updated.")]
        public DateTime? CourseUpdatedOn { get; set; }
        [SwaggerSchema(Description = "The date and time when the venue was last updated.")]
        public DateTime? VenueUpdatedOn { get; set; }        
        [SwaggerSchema(Description = "The unique identifier of the course.")]
        public Guid CourseId { get; set; }
        [SwaggerSchema(Description = "The unique name of the course.")]
        public string CourseName { get; set; }
        [SwaggerSchema(Description = "The type of the course.")]
        public string CourseType { get; set; }
        [SwaggerSchema(Description = "The description of the sector for the course.")]
        public string SectorDescription { get; set; }
        [SwaggerSchema(Description = "The code of the sector for the course.")]
        public string SectorCode { get; set; }
        [SwaggerSchema(Description = "The subject area of the sector for the course.")]
        public string SectorSubjectArea { get; set; }
        [SwaggerSchema(Description = "The education level of the course.")]
        public CourseEducationLevel EducationLevel { get; set; }
        [SwaggerSchema(Description = "The awarding body of the course.")]
        public string AwardingBody { get; set; }
        [SwaggerSchema(Description = "The delivery mode of the course.")]
        public DeliveryMode DeliveryMode { get; set; }
        [SwaggerSchema(Description = "Indicates whether the course has a flexible start date.")]
        public bool FlexibleStartDate { get; set; }
        [SwaggerSchema(Description = "The start date of the course.")]
        public DateTime? StartDate { get; set; }
        [SwaggerSchema(Description = "The website of the course.")]
        public string CourseWebsite { get; set; }
        [SwaggerSchema(Description = "The cost of the course.")]
        public decimal? Cost { get; set; }
        [SwaggerSchema(Description = "The description of the cost of the course.")]
        public string CostDescription { get; set; }
        [SwaggerSchema(Description = "The unit of duration for the course.")]
        public DurationUnit DurationUnit { get; set; }
        [SwaggerSchema(Description = "The value of duration for the course.")]
        public int? DurationValue { get; set; }
        [SwaggerSchema(Description = "The study mode of the course.")]
        public StudyMode StudyMode { get; set; }
        [SwaggerSchema(Description = "The attendance pattern of the course.")]
        public AttendancePattern AttendancePattern { get; set; }
        [SwaggerSchema(Description = "Indicates whether the course is national.")]
        public bool? National { get; set; }
        [SwaggerSchema(Description = "The region of the course.")]
        public string Region { get; set; }
        [SwaggerSchema(Description = "The parent region of the course.")]
        public string ParentRegion { get; set; }
        [SwaggerSchema(Description = "Who the course is for.")]
        public string WhoTheCourseIsFor { get; set; }
        [SwaggerSchema(Description = "The entry requirements for the course.")]
        public string EntryRequirements { get; set; }
        [SwaggerSchema(Description = "What you'll learn in the course.")]
        public string WhatYoullLearn { get; set; }
        [SwaggerSchema(Description = "How you'll learn in the course.")]
        public string HowYoullLearn { get; set; }
        [SwaggerSchema(Description = "What you'll need for the course.")]
        public string WhatYoullNeed { get; set; }
        [SwaggerSchema(Description = "How you'll be assessed in the course.")]
        public string HowYoullBeAssessed { get; set; }
        [SwaggerSchema(Description = "What you can do next after completing the course.")]
        public string WhatYouCanDoNext { get; set; }
        [SwaggerSchema(Description = "The provider name for the course.")]
        public string ProviderName { get; set; }
        [SwaggerSchema(Description = "The website of the course provider.")]
        public string ProviderWebsite { get; set; }
        [SwaggerSchema(Description = "The email of the course provider.")]
        public string ProviderEmail { get; set; }
        [SwaggerSchema(Description = "The phone number of the course provider.")]
        public string ProviderPhoneNumber { get; set; }
        [SwaggerSchema(Description = "The name of the venue for the course.")]
        public string VenueName { get; set; }
        [SwaggerSchema(Description = "The postcode of the venue for the course.")]
        public string Postcode { get; set; }
        [SwaggerSchema(Description = "The first line of the address for the venue.")]
        public string AddressLine1 { get; set; }
        [SwaggerSchema(Description = "The second line of the address for the venue.")]
        public string AddressLine2 { get; set; }
        [SwaggerSchema(Description = "The town of the venue for the course.")]
        public string Town { get; set; }
        [SwaggerSchema(Description = "The county of the venue for the course.")]
        public string County { get; set; }
        [SwaggerSchema(Description = "The latitude of the venue for the course.")]
        public decimal? Latitude { get; set; }
        [SwaggerSchema(Description = "The longitude of the venue for the course.")]
        public decimal? Longitude { get; set; }
        [SwaggerSchema(Description = "The learning aim reference (LARs Code) for the course.")]
        public string LearnAimRef { get; set; }
        [SwaggerSchema(Description = "The title of the learning aim reference (LARs Code) for the course.")]
        public string LearnAimRefTitle  { get; set; }
        [SwaggerSchema(Description = "The qualification level for the course.")]
        public string QualificationLevel { get; set; }
        [SwaggerSchema(Description = "The awarding organisation for the course.")]
        public string AwardingOrganisation  { get; set; }
    }
}
