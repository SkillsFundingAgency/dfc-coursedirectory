using Dfc.Providerportal.FindAnApprenticeship.Models.DAS;
using System.Collections.Generic;

namespace Dfc.Providerportal.FindAnApprenticeship.Interfaces.DAS
{
    public interface IDasStandard
    {
        IDasContact Contact { get; set; }

        List<DasLocationRef> Locations { get; set; }
        string MarketingInfo { get; set; }
        int StandardCode { get; set; }
        string StandardInfoUrl { get; set; }
    }
}
