using Dfc.CourseDirectory.Models.Interface.ApprenticeshipsSearch;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.ApprenticeshipsSearch
{
    public class BulkProviderFrameworkModel : IBulkProviderFrameworkModel
    {
        //  public int id { get; set; }
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
