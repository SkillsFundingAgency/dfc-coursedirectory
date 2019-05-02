using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Apprenticeships
{
    public enum ApprenticeshipType
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Standard Code")]
        StandardCode = 1,
        [Description("Framework Code")]
        FrameworkCode = 2
    }
    public enum ApprenticeshipLocationType
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Classroom based")] // Venue
        ClassroomBased = 1,
        [Description("Employer based")] // Region
        EmployerBased = 2,
        [Description("Classroom based and employer based")] // Venue with added 
        ClassroomBasedAndEmployerBased = 3
    }

    public class Apprenticeship : IApprenticeship
    {
        public Guid id { get; set; } // Cosmos DB id

        public int? ApprenticeshipId { get; set; } // For backwards compatibility with Tribal
        public int? TribalProviderId { get; set; } // For backwards compatibility with Tribal

        public Guid ProviderId { get; set; } // ???
        public int ProviderUKPRN { get; set; } // As we are trying to inforce unique UKPRN per Provider

        // Related like that or by 3 & 2 composite keys
        public ApprenticeshipType ApprenticeshipType { get; set; }
        public Guid? FrameworkId { get; set; }
        public Guid? StandardId { get; set; }

        // It's a duplication of the framework and standard relations
        public int FrameworkCode { get; set; }
        public int ProgType { get; set; } 
        public int PathwayCode { get; set; }
        public int StandardCode { get; set; }
        public int Version { get; set; }

        // Common properties for Standard & Framework
        public string MarketingInformation { get; set; }
        public string Url { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactWebsite { get; set; }

        public IEnumerable<ApprenticeshipLocation> ApprenticeshipLocations { get; set; }

        // Standard auditing properties 
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
