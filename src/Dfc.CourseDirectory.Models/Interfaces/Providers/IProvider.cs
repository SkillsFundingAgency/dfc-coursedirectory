using Dfc.CourseDirectory.Models.Interfaces.Providers;
using Dfc.CourseDirectory.Models.Models.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Providers
{
    public interface IProvider
    {
        Guid id { get; set; }
        string UnitedKingdomProviderReferenceNumber { get; set; }
        string ProviderName { get; set; }
        string ProviderStatus { get; set; }
        IProvidercontact[] ProviderContact { get; set; }
        DateTime ProviderVerificationDate { get; set; }
        bool ProviderVerificationDateSpecified { get; set; }
        bool ExpiryDateSpecified { get; set; }
        object ProviderAssociations { get; set; }
        IProvideralias[] ProviderAliases { get; set; }
        IVerificationdetail[] VerificationDetails { get; set; }
        Status Status { get; set; }
    }
}
