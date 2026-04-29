using System;
using Dfc.CourseDirectory.FindACourseApi.Features.GetCourses;
using Swashbuckle.AspNetCore.Annotations;
namespace Dfc.CourseDirectory.FindACourseApi.Features.GetTLevelUpdates
{
    public class TLevelUpdatesViewModel
    {
        [SwaggerSchema(Description = "The unique identifier of the T Level.")]
        public Guid TLevelId { get; set; }
        [SwaggerSchema(Description = "The type of update for the T Level.")]
        public UpdateType UpdateType { get; set; }
        [SwaggerSchema(Description = "The name of the T Level course.")]
        public string CourseName { get; set; }
        [SwaggerSchema(Description = "The start date of the T Level course.")]
        public DateTime? StartDate { get; set; }
        [SwaggerSchema(Description = "The website URL of the T Level course.")]
        public string CourseWebsite { get; set; }
        [SwaggerSchema(Description = "The description of who the T Level course is for.")]
        public string WhoTheCourseIsFor { get; set; }
        [SwaggerSchema(Description = "The entry requirements for the T Level course.")]
        public string EntryRequirements { get; set; }
        [SwaggerSchema(Description = "The description of what the T Level course will cover.")]
        public string WhatYoullLearn { get; set; }
        [SwaggerSchema(Description = "The description of how you'll learn in the T Level course.")]
        public string HowYoullLearn { get; set; }
        [SwaggerSchema(Description = "The description of what you'll need for the T Level course.")]
        public string WhatYoullNeed { get; set; }
        [SwaggerSchema(Description = "The description of how you'll be assessed in the T Level course.")]
        public string HowYoullBeAssessed { get; set; }
        [SwaggerSchema(Description = "The description of what you can do next after completing the T Level course.")]
        public string WhatYouCanDoNext { get; set; }
        [SwaggerSchema(Description = "The name of the provider offering the T Level course.")]
        public string ProviderName { get; set; }
        [SwaggerSchema(Description = "The website URL of the provider offering the T Level course.")]
        public string ProviderWebsite { get; set; }
        [SwaggerSchema(Description = "The email address of the provider offering the T Level course.")]
        public string ProviderEmail { get; set; }
        [SwaggerSchema(Description = "The phone number of the provider offering the T Level course.")]
        public string ProviderPhoneNumber { get; set; }
        [SwaggerSchema(Description = "The name of the venue where the T Level course is offered.")]
        public string VenueName { get; set; }
        [SwaggerSchema(Description = "The postcode of the venue where the T Level course is offered.")]
        public string Postcode { get; set; }
        [SwaggerSchema(Description = "The first line of the address of the venue where the T Level course is offered.")]
        public string AddressLine1 { get; set; }
        [SwaggerSchema(Description = "The second line of the address of the venue where the T Level course is offered.")]
        public string AddressLine2 { get; set; }
        [SwaggerSchema(Description = "The town where the T Level course is offered.")]
        public string Town { get; set; }
        [SwaggerSchema(Description = "The county where the T Level course is offered.")]
        public string County { get; set; }
        [SwaggerSchema(Description = "The latitude coordinate of the venue where the T Level course is offered.")]
        public decimal? Latitude { get; set; }
        [SwaggerSchema(Description = "The longitude coordinate of the venue where the T Level course is offered.")]
        public decimal? Longitude { get; set; }
        [SwaggerSchema(Description = "The qualification level of the T Level course.")]
        public string QualificationLevel { get; set; }
    }
}
