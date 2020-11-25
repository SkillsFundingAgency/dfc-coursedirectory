using Dfc.Providerportal.FindAnApprenticeship.Models.DAS;
using System.Collections.Generic;

namespace Dfc.Providerportal.FindAnApprenticeship.Interfaces.DAS
{
    public interface IDasProvider
    {
        int? Id { get; set; }
        int? UKPRN { get; set; }
        string Email { get; set; }
        double? EmployerSatisfaction { get; set; }
        List<DasFramework> Frameworks { get; set; }
        double? LearnerSatisfaction { get; set; }
        List<DasLocation> Locations { get; set; }
        string MarketingInfo { get; set; }
        string Name { get; set; }
        string TradingName { get; set; }
        bool NationalProvider { get; set; }
        string Phone { get; set; }
        List<DasStandard> Standards { get; set; }
        
        string Website { get; set; }
    }
}
