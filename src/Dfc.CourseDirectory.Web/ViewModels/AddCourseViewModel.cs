using System;
using System.ComponentModel;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.EntryRequirements;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.FundingOptions;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.HowAssessed;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.HowYouWillLearn;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatWillLearn;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatYouNeed;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhereNext;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class AddCourseViewModel
    {

        public string LearnAimRef { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string AwardOrgCode { get; set; }
        public string LearnAimRefTitle { get; set; }

        public CourseForModel CourseFor { get; set; }

        public EntryRequirementsModel EntryRequirements { get; set; }

        public WhatWillLearnModel WhatWillLearn { get; set; }

        public HowYouWillLearnModel HowYouWillLearn { get; set; }
        public WhatYouNeedModel WhatYouNeed { get; set; }
        public HowAssessedModel HowAssessed { get; set; }
        public WhereNextModel WhereNext { get; set; }
        public FundingOptionsModel FundingOptions { get; set; }

        public CourseMode courseMode { get; set; }

        public Guid CourseId { get; set; }
    }

    public enum AddCoursePage
    {
        None,
        AddCourse,
        AddCourseRun,
        Summary
    }

    public enum CourseMode
    {
        [Description("Add")]
        Add = 0,
        [Description("EditCourse")]
        EditCourse = 1,
        [Description("Copy")]
        Copy = 2,
        [Description("EditCourseRun")]
        EditCourseRun = 3,
        [Description("Review")]
        Review = 4

    }
}
