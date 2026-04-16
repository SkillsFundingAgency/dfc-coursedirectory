using System;
namespace Dfc.CourseDirectory.FindACourseApi.Features.GetTLevelUpdates
{
    public class TLevelUpdatesViewModel
    {
        public Guid TLevelId { get; set; }
        public string UpdateType { get; set; }
        public string CourseName { get; set; }
        public DateTime? StartDate { get; set; }
        public string CourseWebsite { get; set; }
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
        public string QualificationLevel { get; set; }
    }
}
