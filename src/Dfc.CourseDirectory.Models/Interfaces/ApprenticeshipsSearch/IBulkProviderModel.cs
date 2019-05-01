using Dfc.CourseDirectory.Models.Models.ApprenticeshipsSearch;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.ApprenticeshipsSearch
{
    public interface IBulkProviderModel
    {
        int id { get; set; }
        int ukprn { get; set; }
        string name { get; set; }
        string tradingName { get; set; }
        Boolean nationalProvider { get; set; }
        string marketingInfo { get; set; }
        string email { get; set; }
        string website { get; set; }
        string phone { get; set; }
        double? learnerSatisfaction { get; set; }
        double? employerSatisfaction { get; set; }
        List<BulkProviderStandardModel> standards { get; set; }
        List<BulkProviderFrameworkModel> frameworks { get; set; }
        List<BulkProviderLocationModel> locations { get; set; }
    }
}
