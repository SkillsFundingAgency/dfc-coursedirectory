using System.Collections.Generic;

namespace Dfc.CourseDirectory.Models.Models.ApprenticeshipsSearch
{
    public class BulkProviderFrameworkModel
    {
        public int frameworkCode { get; set; }
        public int pathwayCode { get; set; }
        public int progType { get; set; }
        public string marketingInfo { get; set; }
        public string frameworkInfoUrl { get; set; }
        public ProviderContactModel contact { get; set; }
        public List<ApprenticeshipLocationModel> locations { get; set; }

        public BulkProviderFrameworkModel()
        {
            contact = new ProviderContactModel();
        }
    }
}
