using System.Collections.Generic;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.DAS;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.DAS
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
