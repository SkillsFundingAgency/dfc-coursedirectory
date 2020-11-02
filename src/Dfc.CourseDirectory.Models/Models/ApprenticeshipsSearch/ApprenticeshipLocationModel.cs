using System.Collections.Generic;

namespace Dfc.CourseDirectory.Models.Models.ApprenticeshipsSearch
{
    public class ApprenticeshipLocationModel
    {
        public int id { get; set; }
        public List<string> deliveryModes { get; set; }
        public int? radius { get; set; }

        public ApprenticeshipLocationModel()
        {
            deliveryModes = new List<string>();
        }
    }
}
