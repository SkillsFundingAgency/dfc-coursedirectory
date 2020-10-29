using System;
using Dfc.CourseDirectory.FindACourseApi.Interfaces.Faoc;

namespace Dfc.CourseDirectory.FindACourseApi.Models.Search.Faoc
{
    public class AzureSearchOnlineCourse : IAzureSearchOnlineCourse
    {
        public string Id { get; set; }  // Mandatory
        public string QualificationCourseTitle { get; set; }
        public string LearnAimRef { get; set; }
        public string NotionalNVQLevelv2 { get; set; }  // Mandatory
        public string AwardOrgCode { get; set; }
        public string QualificationType { get; set; }
        public string CourseDescription { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string WhatYoullNeed { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhereNext { get; set; }
        public bool AdultEducationBudget { get; set; }
        public bool AdvancedLearnerLoan { get; set; }
        public string CourseName { get; set; }
        public DateTime? StartDate { get; set; }
        public bool FlexibleStartDate { get; set; }  // Mandatory
        public string CourseWebsite { get; set; }
        public int? ProviderUKPRN { get; set; }
        public string ProviderName { get; set; }
        public string ProviderAddressLine1 { get; set; }
        public string ProviderAddressLine2 { get; set; }
        public string ProviderTown { get; set; }
        public string ProviderPostcode { get; set; }
        public string ProviderCounty { get; set; }
        public string ProviderEmail { get; set; }
        public string ProviderTelephone { get; set; }
        public string ProviderFax { get; set; }
        public string ProviderWebsite { get; set; }
        public decimal? ProviderLearnerSatisfaction { get; set; }
        public decimal? ProviderEmployerSatisfaction { get; set; }

        // For referencing back to CD, if required
        public Guid? CourseId { get; set; }
        public Guid? CourseRunId { get; set; }
    }
}