using Dfc.Providerportal.FindAnApprenticeship.Models.DAS;
using System.Collections.Generic;

namespace Dfc.Providerportal.FindAnApprenticeship.Interfaces.DAS
{
    public interface IDasFramework
    {
        IDasContact Contact { get; set; }

        int FrameworkCode { get; set; }

        int? ProgType { get; set; }

        List<DasLocationRef> Locations { get; set; }
        int? PathwayCode { get; set; }

        string FrameworkInfoUrl { get; set; }

        string MarketingInfo { get; set; }
    }
}
