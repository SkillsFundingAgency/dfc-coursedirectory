﻿using System;
using Dfc.CourseDirectory.WebV2.ViewModels;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.RequestModels
{
    public class AddCourseSection1RequestModel
    {
        public string LearnAimRefTitle { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string AwardOrgCode { get; set; }
        public string LearnAimRef { get; set; }

        public string CourseFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatWillLearn { get; set; }
        public string HowYouWillLearn { get; set; }
        public string WhatYouNeed { get; set; }
        public string HowAssessed { get; set; }
        public string WhereNext { get; set; }
        public bool National { get; set; }
        public bool AdultEducationBudget { get; set; }
        public bool AdvancedLearnerLoan { get; set; }

        public CourseMode CourseMode { get; set; }

        public Guid? CourseId { get; set; }
        public Guid? CourseRunId { get; set; }
    }
}
