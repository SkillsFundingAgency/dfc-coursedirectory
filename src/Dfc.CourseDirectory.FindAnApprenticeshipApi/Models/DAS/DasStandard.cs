using System.Collections.Generic;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.DAS;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.DAS
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
