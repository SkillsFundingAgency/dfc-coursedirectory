using Dfc.CourseDirectory.Models.Interfaces.ApprenticeshipsSearch;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.ApprenticeshipsSearch
{
    public class BulkProviderLocationModel : IBulkProviderLocationModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public LocationAddressModel address { get; set; }
        public string email { get; set; }
        public string website { get; set; }
        public string phone { get; set; }

        public BulkProviderLocationModel()
        {
            address = new LocationAddressModel();
        }
    }
}
