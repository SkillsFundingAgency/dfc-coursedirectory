using System;
using System.ComponentModel;
using Dfc.Providerportal.FindAnApprenticeship.Models.Provider;

namespace Dfc.Providerportal.FindAnApprenticeship.Models.Providers
{
    public class Provider
    {
        public Guid Id { get; set; }
        public string UnitedKingdomProviderReferenceNumber { get; set; }
        public string ProviderName { get; set; }
        public string CourseDirectoryName { get; set; }
        public string ProviderStatus { get; set; }
        public Providercontact[] ProviderContact { get; set; }
        public DateTime? ProviderVerificationDate { get; set; }
        public bool? ProviderVerificationDateSpecified { get; set; }
        public bool? ExpiryDateSpecified { get; set; }
        public object ProviderAssociations { get; set; }
        public Provideralias[] ProviderAliases { get; set; }
        public Verificationdetail[] VerificationDetails { get; set; }
        public Status Status { get; set; }
        public int? ProviderId { get; set; }
        public int? UPIN { get; set; }
        public string TradingName { get; set; }
        public bool NationalApprenticeshipProvider { get; set; }
        public string MarketingInformation { get; set; }
        public string Alias { get; set; }
        public ProviderType ProviderType { get; set; }
        public ProviderDisplayNameSource DisplayNameSource { get; set; }

        public string Name =>
            DisplayNameSource == ProviderDisplayNameSource.TradingName && !string.IsNullOrEmpty(Alias)
                ? Alias
                : ProviderName;
    }

    public enum Status
    {
        Registered = 0,
        Onboarded = 1,
        Unregistered = 2
    }

    public enum ProviderType
    {
        Undefined = 0,
        [Description("F.E.")]
        Fe = 1,
        [Description("Apprenticeships")]
        Apprenticeship = 2
    }

    public enum ProviderDisplayNameSource
    {
        ProviderName = 0,
        TradingName = 1
    }
}