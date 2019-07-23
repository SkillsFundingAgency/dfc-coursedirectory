using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Apprenticeships
{
    public interface ILocationRef
    {
        int ID { get; set; }

        List<string> DeliveryModes { get; set; }

        string MarketingInfo { get; set; }

        int Radius { get; set; }

        string StandardInfoUrl { get; set; }

    }
}
