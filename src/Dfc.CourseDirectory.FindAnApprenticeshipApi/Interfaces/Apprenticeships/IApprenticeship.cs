using Dfc.Providerportal.FindAnApprenticeship.Models;
using Dfc.Providerportal.FindAnApprenticeship.Models.Enums;
using System;
using System.Collections.Generic;

namespace Dfc.Providerportal.FindAnApprenticeship.Interfaces.Apprenticeships
{
    public interface IApprenticeship
    {
        Guid id { get; set; } // Cosmos DB id

        int? ApprenticeshipId { get; set; } // For backwards compatibility with Tribal
        int? TribalProviderId { get; set; } // For backwards compatibility with Tribal
        string ApprenticeshipTitle { get; set; }
        Guid ProviderId { get; set; } // ???
        int ProviderUKPRN { get; set; } // As we are trying to inforce unique UKPRN per Provider

        // Related like that or by 3 & 2 composite keys
        ApprenticeshipType ApprenticeshipType { get; set; }
        Guid? FrameworkId { get; set; }
        Guid? StandardId { get; set; }

        // It's a duplication of the framework and standard relations.
        int? FrameworkCode { get; set; }
        int? ProgType { get; set; }
        int? PathwayCode { get; set; }
        int? StandardCode { get; set; }
        int? Version { get; set; }
        // Common properties for Standard & Framework
        string MarketingInformation { get; set; }
        string Url { get; set; }
        string ContactTelephone { get; set; }
        string ContactEmail { get; set; }
        string ContactWebsite { get; set; }

        List<ApprenticeshipLocation> ApprenticeshipLocations { get; set; }

        // Standard auditing properties 
        RecordStatus RecordStatus { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedDate { get; set; }
        string UpdatedBy { get; set; }

        string NotionalNVQLevelv2 { get; set; }
        List<BulkUploadError> BulkUploadErrors { get; set; }
    }
}
