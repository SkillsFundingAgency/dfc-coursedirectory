using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Search.Models;

namespace Dfc.CourseDirectory.FindACourseApi.Features.TLevelDetail
{
    public class TLevelDetailViewModel
    {
        public FindACourseOfferingType OfferingType { get; set; }
        public Guid TLevelId { get; set; }
        public QualificationViewModel Qualification { get; set; }
        public ProviderViewModel Provider { get; set; }
        public string WhoFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhatYouCanDoNext { get; set; }
        public string YourReference { get; set; }
        public string Website { get; set; }
        public DateTime StartDate { get; set; }
        public IReadOnlyCollection<TLevelLocationViewModel> Locations { get; set; }
        public CourseDeliveryMode DeliveryMode { get; set; }
        public CourseAttendancePattern AttendancePattern { get; set; }
        public CourseStudyMode StudyMode { get; set; }
        public CourseDurationUnit DurationUnit { get; set; }
        public int? DurationValue { get; set; }
        public decimal? Cost { get; set; }
    }
}
