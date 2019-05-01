using Dfc.CourseDirectory.Models.Interfaces.ApprenticeshipsSearch;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.ApprenticeshipsSearch
{
    public class BulkProviderStandardModel : IBulkProviderStandardModel
    {
        //  public int id { get; set; }
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
