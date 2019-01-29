using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.CourseTextService;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.CourseTextService
{
    public class CourseTextSearchResult : ValueObject<CourseTextSearchResult>, ICourseTextSearchResult
    {
        public Guid ID { get; set; }
        public string QualificationCourseTitle { get; set; }
        public string LearnAimRef { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string TypeName { get; set; }
        public string AwardOrgCode { get; set; }
        public string CountOfOpportunities { get; set; }
        public string CourseDescription { get; set; }
        public string EntryRequirments { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string WhatYoullNeed { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhereNext { get; set; }

        public CourseTextSearchResult(
            Guid id,
            string qualificationCourseTitle,
            string learnAimRef,
            string notionalNVQLevelV2,
            string typeName,
            string awardOrgCode,
            string countofOpportunities,
            string courseDescription,
            string entryRequirments,
            string whatYoullLearn,
            string howYoullLearn,
            string whatYoullNeed,
            string howYoullBeAssessed,
            string whereNext)
        {
            Throw.IfNull(ID, nameof(ID));
            Throw.IfNullOrWhiteSpace(qualificationCourseTitle, nameof(qualificationCourseTitle));
            Throw.IfNullOrWhiteSpace(learnAimRef, nameof(learnAimRef));
            Throw.IfNullOrWhiteSpace(notionalNVQLevelV2, nameof(notionalNVQLevelV2));
            Throw.IfNullOrWhiteSpace(typeName, nameof(typeName));
            Throw.IfNullOrWhiteSpace(awardOrgCode, nameof(awardOrgCode));
            Throw.IfNullOrWhiteSpace(countofOpportunities, nameof(countofOpportunities));
            Throw.IfNullOrWhiteSpace(courseDescription, nameof(courseDescription));
            Throw.IfNullOrWhiteSpace(entryRequirments, nameof(entryRequirments));
            Throw.IfNullOrWhiteSpace(whatYoullLearn, nameof(whatYoullLearn));
            Throw.IfNullOrWhiteSpace(howYoullLearn, nameof(howYoullLearn));
            Throw.IfNullOrWhiteSpace(whatYoullNeed, nameof(whatYoullNeed));
            Throw.IfNullOrWhiteSpace(howYoullBeAssessed, nameof(howYoullBeAssessed));
            Throw.IfNullOrWhiteSpace(whereNext, nameof(whereNext));
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return ID;
            yield return QualificationCourseTitle;
            yield return LearnAimRef;
            yield return NotionalNVQLevelv2;
            yield return TypeName;
            yield return AwardOrgCode;
            yield return CountOfOpportunities;
            yield return CourseDescription;
            yield return EntryRequirments;
            yield return WhatYoullLearn;
            yield return HowYoullLearn;
            yield return WhatYoullNeed;
            yield return HowYoullBeAssessed;
            yield return WhereNext;
        }

    }
}
