using Dfc.CourseDirectory.Models.Models.ApprenticeshipsSearch;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.ApprenticeshipsSearch
{
    public interface IBulkProviderStandardModel
    {
        //  int id { get; set; }
        int standardCode { get; set; }
        string marketingInfo { get; set; }
        string standardInfoUrl { get; set; }
        ProviderContactModel contact { get; set; }
        List<ApprenticeshipLocationModel> locations { get; set; }
    }
}
