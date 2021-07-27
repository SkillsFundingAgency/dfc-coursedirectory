using System;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Services.Models.Regions;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseRegion;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.SelectVenue;

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
        public ChooseRegionModel ChooseRegion { get; set; }

        public CourseDeliveryMode DeliveryMode { get; set; }
        public string CourseProviderReference { get; set; }
        public string Url { get; set; }
        public bool AdultEducationBudget { get; set; }
        public bool AdvancedLearnerLoan { get; set; }
        public CourseDurationUnit? DurationUnit { get; set; }
        public CourseStudyMode? StudyMode { get; set; }
        public CourseAttendancePattern? AttendanceMode { get; set; }

        public CourseMode CourseMode { get; set; }

        public Guid? CourseId { get; set; }
        public Guid CourseRunId { get; set; }
    }
}
