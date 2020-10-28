using System;

namespace Dfc.ProviderPortal.FindACourse.Interfaces.Faoc
{
    public interface IAzureSearchOnlineCourse
    {
        string Id { get; set; } // Mandatory
        string QualificationCourseTitle { get; set; }
        string LearnAimRef { get; set; }
        string NotionalNVQLevelv2 { get; set; } // Mandatory
        string AwardOrgCode { get; set; }
        string QualificationType { get; set; }
        string CourseDescription { get; set; }
        string EntryRequirements { get; set; }
        string WhatYoullLearn { get; set; }
        string HowYoullLearn { get; set; }
        string WhatYoullNeed { get; set; }
        string HowYoullBeAssessed { get; set; }
        string WhereNext { get; set; }
        bool AdultEducationBudget { get; set; }
        bool AdvancedLearnerLoan { get; set; }
        string CourseName { get; set; }
        DateTime? StartDate { get; set; }
        bool FlexibleStartDate { get; set; } // Mandatory
        string CourseWebsite { get; set; }
        int? ProviderUKPRN { get; set; }
        string ProviderName { get; set; }
        string ProviderAddressLine1 { get; set; }
        string ProviderAddressLine2 { get; set; }
        string ProviderTown { get; set; }
        string ProviderPostcode { get; set; }
        string ProviderCounty { get; set; }
        string ProviderEmail { get; set; }
        string ProviderTelephone { get; set; }
        string ProviderFax { get; set; }
        string ProviderWebsite { get; set; }
        decimal? ProviderLearnerSatisfaction { get; set; }
        decimal? ProviderEmployerSatisfaction { get; set; }
        Guid? CourseId { get; set; }
        Guid? CourseRunId { get; set; }
    }
}