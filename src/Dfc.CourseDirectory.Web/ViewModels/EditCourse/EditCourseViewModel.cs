using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.EntryRequirements;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.HowAssessed;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.HowYouWillLearn;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatWillLearn;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatYouNeed;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhereNext;

namespace Dfc.CourseDirectory.Web.ViewModels.EditCourse
{
    public class EditCourseViewModel
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

        public Guid? CourseId { get; set; }
        public Guid? CourseRunId { get; set; }
        public string QualificationType { get; set; }

        public bool? AdultEducationBudget { get; set; }
        public bool? AdvancedLearnerLoan{ get; set; }

        public PublishMode Mode { get; set; }
        public string CourseName { get; set; }

       // public string NotionalNVQLevelv2 { get; set; }

    }   
}
