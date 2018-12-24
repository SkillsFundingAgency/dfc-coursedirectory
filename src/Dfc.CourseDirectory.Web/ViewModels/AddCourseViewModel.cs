using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.EntryRequirements;
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
    }
}
