using Dfc.CourseDirectory.Models.Models.ApprenticeshipsSearch;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interface.ApprenticeshipsSearch
{
    public interface IBulkProviderFrameworkModel
    {
        //  int id { get; set; }
        int frameworkCode { get; set; }
        int pathwayCode { get; set; }
        int progType { get; set; }
        string marketingInfo { get; set; }
        string frameworkInfoUrl { get; set; }
        ProviderContactModel contact { get; set; }
        List<ApprenticeshipLocationModel> locations { get; set; }
    }
}
