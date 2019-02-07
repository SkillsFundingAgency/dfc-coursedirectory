using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.EntryRequirements;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.HowAssessed;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.HowYouWillLearn;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.SelectVenue;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatWillLearn;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatYouNeed;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.WhereNext;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.SelectRegion;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class AddCourseDetailsViewModel
    {
        public string LearnAimRef { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string AwardOrgCode { get; set; }
        public string LearnAimRefTitle { get; set; }

        public int ProviderUKPRN { get; set; }

        public string CourseFor { get; set; }

        public string EntryRequirements { get; set; }

        public string WhatWillLearn { get; set; }

        public string HowYouWillLearn { get; set; }
        public string WhatYouNeed { get; set; }
        public string HowAssessed { get; set; }
        public string WhereNext { get; set; }


        public string CourseName { get; set; }

        public DateTime StartDate { get; set; }

        public StartDateType StartDateType { get; set; }

        public string Day { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public string DurationLength { get; set; }


        public string Cost { get; set; }


        public string CostDescription { get; set; }

        public SelectVenueModel SelectVenue { get; set; }
        public SelectRegionModel SelectRegion { get; set; }

        public DeliveryMode DeliveryMode { get; set; }
        public string CourseProviderReference { get; set; }
        public string Url { get; set; }
        public bool AdvancedLearnerLoan { get; set; }
        public DurationUnit DurationUnit { get; set; }
        public StudyMode StudyMode { get; set; }
        public AttendancePattern AttendanceMode { get; set; }

        public CourseMode CourseMode { get; set; }

        public Guid? CourseId { get; set; }
        public Guid CourseRunId { get; set; }

      
    }
}
