using Dfc.Providerportal.FindAnApprenticeship.Interfaces.Apprenticeships;
using Dfc.Providerportal.FindAnApprenticeship.Models.Enums;
using System;
using System.Collections.Generic;

namespace Dfc.Providerportal.FindAnApprenticeship.Models
{
    public class Apprenticeship : IApprenticeship
    {
        public Guid id { get; set; } // Cosmos DB id

        public int? ApprenticeshipId { get; set; } // For backwards compatibility with Tribal
        public int? TribalProviderId { get; set; } // For backwards compatibility with Tribal

        public string ApprenticeshipTitle { get; set; }
        public Guid ProviderId { get; set; } // ???
        public int ProviderUKPRN { get; set; } // As we are trying to inforce unique UKPRN per Provider

        // Related like that or by 3 & 2 composite keys
        public ApprenticeshipType ApprenticeshipType { get; set; }
        public Guid? FrameworkId { get; set; }
        public Guid? StandardId { get; set; }

        // It's a duplication of the framework and standard relations
        public int? FrameworkCode { get; set; }
        public int? ProgType { get; set; } 
        public int? PathwayCode { get; set; }
        public int? StandardCode { get; set; }
        public int? Version { get; set; }

        // Common properties for Standard & Framework
        public string MarketingInformation { get; set; }
        public string Url { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactWebsite { get; set; }
        public List<ApprenticeshipLocation> ApprenticeshipLocations { get; set; }

        // Standard auditing properties 
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        
        public List<BulkUploadError> BulkUploadErrors { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
    }
}
