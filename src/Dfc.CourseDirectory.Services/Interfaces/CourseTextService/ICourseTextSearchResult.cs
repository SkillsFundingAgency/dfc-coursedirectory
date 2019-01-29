using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.Interfaces.CourseTextService
{
    interface ICourseTextSearchResult
    {
        Guid ID { get; }
        string QualificationCourseTitle { get; }
        string LearnAimRef { get; }
        string NotionalNVQLevelv2 { get; }
        string TypeName { get; }
        string AwardOrgCode { get; }
        string CountOfOpportunities { get; }
        string CourseDescription { get; }
        string EntryRequirments { get; }
        string WhatYoullLearn { get; }
        string HowYoullLearn { get; }
        string WhatYoullNeed { get; }
        string HowYoullBeAssessed { get; }
        string WhereNext { get; }
    }
}
