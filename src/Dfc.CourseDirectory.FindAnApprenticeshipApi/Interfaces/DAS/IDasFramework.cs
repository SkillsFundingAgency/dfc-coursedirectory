using System.Collections.Generic;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.DAS;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.DAS
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
