
using System;
using System.ComponentModel;
using System.Text;
using System.Collections.Generic;
using Dfc.CourseDirectory.FindACourseApi.Interfaces;


namespace Dfc.CourseDirectory.FindACourseApi.Models
{
    public class Provider
    {
        public Guid id { get; set; }
        public string UnitedKingdomProviderReferenceNumber { get; set; }
        public string ProviderName { get; set; }
        public string CourseDirectoryName { get; set; }
        public string ProviderStatus { get; set; }
        public dynamic /*IProvidercontact[]*/ ProviderContact { get; set; }
        public DateTime ProviderVerificationDate { get; set; }
        public bool ProviderVerificationDateSpecified { get; set; }
        public bool ExpiryDateSpecified { get; set; }
        public object ProviderAssociations { get; set; }
        public dynamic /*IProvideralias[]*/ ProviderAliases { get; set; }
        public dynamic /*IVerificationdetail[]*/ VerificationDetails { get; set; }
        public Status Status { get; set; }

        // Apprenticeship related
        public int? ProviderId { get; set; }
        public int? UPIN { get; set; } // Needed to get LearnerSatisfaction & EmployerSatisfaction from FEChoices
        public string TradingName { get; set; }
        public bool NationalApprenticeshipProvider { get; set; }
        public string MarketingInformation { get; set; }
        public string Alias { get; set; }

        // Bulk course upload
        public dynamic /*BulkUploadStatus*/ BulkUploadStatus { get; set; }

        //public Provider(Providercontact[] providercontact, Provideralias[] provideraliases, Verificationdetail[] verificationdetails)
        //{
        //    ProviderContact = providercontact;
        //    ProviderAliases = provideraliases;
        //    VerificationDetails = verificationdetails;
        //}

        public ProviderType ProviderType { get; set; }
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
        FE = 1,
        [Description("Apprenticeships")]
        Apprenticeship = 2,
        [Description("Both")]
        Both = FE | Apprenticeship
    }

    public class Providercontact
    {
        public string ContactType { get; set; }
        public Contactaddress ContactAddress { get; set; }
        public Contactpersonaldetails ContactPersonalDetails { get; set; }
        public object ContactRole { get; set; }
        public string ContactTelephone1 { get; set; }
        public object ContactTelephone2 { get; set; }
        public string ContactFax { get; set; }
        public string ContactWebsiteAddress { get; set; }
        public string ContactEmail { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class Contactaddress
    {
        public SAON SAON { get; set; }
        public PAON PAON { get; set; }
        public string StreetDescription { get; set; }
        public object UniqueStreetReferenceNumber { get; set; }
        public string Locality { get; set; }
        public string[] Items { get; set; }
        public int[] ItemsElementName { get; set; }
        public object PostTown { get; set; }
        public string PostCode { get; set; }
        public object UniquePropertyReferenceNumber { get; set; }
    }

    public class SAON
    {
        public string Description { get; set; }
    }

    public class PAON
    {
        public string Description { get; set; }
    }

    public class Contactpersonaldetails
    {
        public string[] PersonNameTitle { get; set; }
        public string[] PersonGivenName { get; set; }
        public string PersonFamilyName { get; set; }
        public object PersonNameSuffix { get; set; }
        public object PersonRequestedName { get; set; }
    }
}
