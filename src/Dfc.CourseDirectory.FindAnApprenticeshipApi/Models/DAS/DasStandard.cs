using Dfc.Providerportal.FindAnApprenticeship.Interfaces.DAS;
using System.Collections.Generic;

namespace Dfc.Providerportal.FindAnApprenticeship.Models.DAS
{
    public class DasStandard : IDasStandard
    {
        public int StandardCode { get; set; }
        public string MarketingInfo { get; set; }
        public string StandardInfoUrl { get; set; }
        public IDasContact Contact { get; set; }
        public List<DasLocationRef> Locations { get; set; }
      
        
        
    }
}
