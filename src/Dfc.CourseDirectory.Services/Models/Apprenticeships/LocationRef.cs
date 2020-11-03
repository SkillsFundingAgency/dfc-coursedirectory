using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services.Models.Apprenticeships
{
    public class LocationRef
    {
        public int ID { get; set; }
        public List<string> DeliveryModes { get; set; }
        public string MarketingInfo { get; set; }
        public int? Radius { get; set; }
        public string StandardInfoUrl { get; set; }
    }
}
