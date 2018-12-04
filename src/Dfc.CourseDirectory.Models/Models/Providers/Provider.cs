using Dfc.CourseDirectory.Models.Interfaces.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Providers
{
    public class Provider : IProvider
    {
        public string UnitedKingdomProviderReferenceNumber { get; set; }
        public string ProviderName { get; set; }
        public string ProviderStatus { get; set; }
        public IProvidercontact[] ProviderContact { get; set; }
        public DateTime ProviderVerificationDate { get; set; }
        public bool ProviderVerificationDateSpecified { get; set; }
        public bool ExpiryDateSpecified { get; set; }
        public object ProviderAssociations { get; set; }
        public IProvideralias[] ProviderAliases { get; set; }
        public IVerificationdetail[] VerificationDetails { get; set; }

        public Provider(Providercontact[] providercontact, Provideralias[] provideraliases, Verificationdetail[] verificationdetails)
        {
            ProviderContact = providercontact;
            ProviderAliases = provideraliases;
            VerificationDetails = verificationdetails;

        }
    }
}
