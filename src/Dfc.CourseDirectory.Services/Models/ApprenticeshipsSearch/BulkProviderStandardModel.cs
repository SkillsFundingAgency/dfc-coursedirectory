using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services.Models.ApprenticeshipsSearch
{
    public class BulkProviderStandardModel
    {
        public int standardCode { get; set; }
        public string marketingInfo { get; set; }
        public string standardInfoUrl { get; set; }
        public ProviderContactModel contact { get; set; }
        public List<ApprenticeshipLocationModel> locations { get; set; }

        public BulkProviderStandardModel()
        {
            contact = new ProviderContactModel();
        }
    }
}
