using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.EntryRequirements;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatWillLearn;

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
    }
}
