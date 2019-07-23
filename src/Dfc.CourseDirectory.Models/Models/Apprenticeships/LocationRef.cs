using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Apprenticeships
{
    public class LocationRef : ILocationRef
    {
        public int ID { get; set; }
        public List<string> DeliveryModes { get; set; }

        public string MarketingInfo { get; set; }

        public int Radius { get; set; }

        public string StandardInfoUrl { get; set; }
    }
}
