using Dfc.CourseDirectory.Models.Models.ApprenticeshipsSearch;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.ApprenticeshipsSearch
{
    public interface IBulkProviderLocationModel
    {
        int id { get; set; }
        string name { get; set; }
        LocationAddressModel address { get; set; }
        string email { get; set; }
        string website { get; set; }
        string phone { get; set; }
    }
}
