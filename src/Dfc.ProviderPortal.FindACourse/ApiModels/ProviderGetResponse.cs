using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.ProviderPortal.FindACourse.Models;

namespace Dfc.ProviderPortal.FindACourse.ApiModels
{
    public class ProviderGetResponse
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
}
