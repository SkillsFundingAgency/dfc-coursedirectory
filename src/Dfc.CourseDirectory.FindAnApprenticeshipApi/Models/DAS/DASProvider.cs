using Dfc.Providerportal.FindAnApprenticeship.Interfaces.DAS;
using System.Collections.Generic;

namespace Dfc.Providerportal.FindAnApprenticeship.Models.DAS
{
    public class DasProvider : IDasProvider
    {
        public DasProvider()
        {
            
        }
        public int? Id { get; set; }
        public int? UKPRN { get; set; }
        public string Name { get; set; }
        public string TradingName { get; set; }
        public bool NationalProvider { get; set; }
        public string MarketingInfo { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Phone { get; set; }
        public double? LearnerSatisfaction { get; set; }
        public double? EmployerSatisfaction { get; set; }
        public List<DasStandard> Standards { get; set; }
        public List<DasFramework> Frameworks { get; set; }
        public List<DasLocation> Locations { get; set; }

    }
}
