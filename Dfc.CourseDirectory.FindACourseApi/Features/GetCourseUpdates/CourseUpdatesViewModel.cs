using System;
using Dfc.CourseDirectory.FindACourseApi.Features.GetCourses;

namespace Dfc.CourseDirectory.FindACourseApi.Features.GetCourseUpdates
{
    public class CourseUpdatesViewModel
    {
        public Guid Id { get; set; }
        public UpdateType UpdateType { get; set; }
        public CourseRunStatus CourseRunStatus { get; set; }
        public string ContactType { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public DateTime? CourseUpdatedOn { get; set; }
        public DateTime? VenueUpdatedOn { get; set; }
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
}
