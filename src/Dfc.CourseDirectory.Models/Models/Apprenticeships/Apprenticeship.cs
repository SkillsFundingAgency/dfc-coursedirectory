using Dfc.CourseDirectory.Models.Enums;
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

    public class Apprenticeship
    {
        public Guid id { get; set; } // Cosmos DB id

        public int? ApprenticeshipId { get; set; } // For backwards compatibility with Tribal
        public int? TribalProviderId { get; set; } // For backwards compatibility with Tribal

        public Guid ProviderId { get; set; } // ???
        public string ProviderUKPRN { get; set; } // As we are trying to inforce unique UKPRN per Provider

        // Related like that or by 3 & 2 composite keys
        public ApprenticeshipType ApprenticeshipType { get; set; }
        public Guid? FrameworkId { get; set; }
        public Guid? StandardId { get; set; }

        // Common properties for Standard & Framework
        public string MarketingInformation { get; set; }
        public string Url { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactWebsite { get; set; }

        // Standard auditing properties 
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
