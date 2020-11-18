using System;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Services.Models.Providers
{
    public class Provider
    {
        public Guid id { get; set; }
        public string UnitedKingdomProviderReferenceNumber { get; set; }
        public string ProviderName { get; set; }
        public string CourseDirectoryName { get; set; }
        public string ProviderStatus { get; set; }
        public Providercontact[] ProviderContact { get; set; }
        public DateTime ProviderVerificationDate { get; set; }
        public bool ProviderVerificationDateSpecified { get; set; }
        public bool ExpiryDateSpecified { get; set; }
        public object ProviderAssociations { get; set; }
        public Provideralias[] ProviderAliases { get; set; }
        public Verificationdetail[] VerificationDetails { get; set; }
        public ProviderStatus Status { get; set; }
        public int? ProviderId { get; set; }
        public int? UPIN { get; set; }
        public string TradingName { get; set; }
        public bool NationalApprenticeshipProvider { get; set; }
        public string MarketingInformation { get; set; }
        public string Alias { get; set; }
        public BulkUploadStatus BulkUploadStatus { get; set; }
        public BulkUploadStatus ApprenticeshipBulkUploadStatus { get; set; }
        public ProviderType ProviderType { get; set; }

        public Provider(Providercontact[] providercontact, Provideralias[] provideraliases, Verificationdetail[] verificationdetails)
        {
            ProviderContact = providercontact;
            ProviderAliases = provideraliases;
            VerificationDetails = verificationdetails;
        }
    }

    public enum Status
    {
        Registered = 0,
        Onboarded = 1,
        Unregistered = 2
    }

    [Flags]
    public enum ProviderType
    {
        Undefined = 0,
        [Description("F.E.")]
        FE = 1,
        [Description("Apprenticeships")]
        Apprenticeship = 2
    }
}
