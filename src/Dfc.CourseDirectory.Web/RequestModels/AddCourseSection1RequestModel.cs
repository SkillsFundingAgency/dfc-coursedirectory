using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.RequestModels
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
    }
}
