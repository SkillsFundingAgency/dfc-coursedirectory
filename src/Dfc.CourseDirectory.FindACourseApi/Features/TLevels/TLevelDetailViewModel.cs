using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Search.Models;

namespace Dfc.CourseDirectory.FindACourseApi.Features.TLevels
{
    public class TLevelDetailViewModel
    {
        public FindACourseOfferingType OfferingType => FindACourseOfferingType.TLevel;
        public Guid TLevelId { get; set; }
        public Guid TLevelDefinitionId { get; set; }
        public QualificationViewModel Qualification { get; set; }
        public ProviderViewModel Provider { get; set; }
        public string WhoFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhatYouCanDoNext { get; set; }
        public string Website { get; set; }
        public DateTime StartDate { get; set; }
        public IReadOnlyCollection<TLevelLocationViewModel> Locations { get; set; }
        public CourseDeliveryMode DeliveryMode => CourseDeliveryMode.ClassroomBased;
        public CourseAttendancePattern AttendancePattern => CourseAttendancePattern.Daytime;
        public CourseStudyMode StudyMode => CourseStudyMode.FullTime;
        public CourseDurationUnit DurationUnit => CourseDurationUnit.Years;
        public int? DurationValue => 2;
        public decimal? Cost => null;
        public string CostDescription => "T Levels are currently only available to 16-19 year olds. Contact us for details of other suitable courses.";
    }
}
