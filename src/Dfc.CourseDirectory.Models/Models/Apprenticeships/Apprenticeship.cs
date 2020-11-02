using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;

namespace Dfc.CourseDirectory.Models.Models.Apprenticeships
{
    public enum ApprenticeshipMode
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Add")]
        Add = 1,
        [Description("EditApprenticeship")]
        EditApprenticeship = 2,
        [Description("EditYourApprenticeships")]
        EditYourApprenticeships = 3,
        [Description("DeleteYourAprrenticeships")]
        DeleteYourAprrenticeships = 4
    }

    public enum ApprenticeshipType
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Standard")]
        StandardCode = 1,
        [Description("Framework")]
        FrameworkCode = 2
    }

    public enum ApprenticeshipLocationType
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Classroom based")]
        ClassroomBased = 1,
        [Description("Employer based")]
        EmployerBased = 2,
        [Description("Classroom based and employer based")]
        ClassroomBasedAndEmployerBased = 3
    }

    public class Apprenticeship
    {
        public Guid id { get; set; }
        public int? ApprenticeshipId { get; set; }
        public int? TribalProviderId { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public Guid ProviderId { get; set; }
        public int ProviderUKPRN { get; set; }
        public ApprenticeshipType ApprenticeshipType { get; set; }
        public Guid? FrameworkId { get; set; }
        public Guid? StandardId { get; set; }
        public int? FrameworkCode { get; set; }
        public int? ProgType { get; set; } 
        public int? PathwayCode { get; set; }
        public int? StandardCode { get; set; }
        public int? Version { get; set; }
        public string MarketingInformation { get; set; }
        public string Url { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactWebsite { get; set; }
        public List<ApprenticeshipLocation> ApprenticeshipLocations { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public List<BulkUploadError> BulkUploadErrors { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public IEnumerable<string> ValidationErrors { get; set; }
        public IEnumerable<string> LocationValidationErrors { get; set; }
    }
}
