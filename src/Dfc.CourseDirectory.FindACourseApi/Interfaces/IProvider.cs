
using System;
using Dfc.CourseDirectory.FindACourseApi.Models;


namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface IProvider
    {
        Guid id { get; set; }
        string UnitedKingdomProviderReferenceNumber { get; set; }
        string ProviderName { get; set; }
        string CourseDirectoryName { get; set; }
        string ProviderStatus { get; set; }
        dynamic /*IProvidercontact[]*/ ProviderContact { get; set; }
        DateTime ProviderVerificationDate { get; set; }
        bool ProviderVerificationDateSpecified { get; set; }
        bool ExpiryDateSpecified { get; set; }
        object ProviderAssociations { get; set; }
        dynamic /*IProvideralias[]*/ ProviderAliases { get; set; }
        dynamic /*IVerificationdetail[]*/ VerificationDetails { get; set; }
        Status Status { get; set; }

        // Apprenticeship related
        int? ProviderId { get; set; }
        int? UPIN { get; set; } // Needed to get LearnerSatisfaction & EmployerSatisfaction from FEChoices
        string TradingName { get; set; }
        bool NationalApprenticeshipProvider { get; set; }
        string MarketingInformation { get; set; }
        string Alias { get; set; }
    }
}