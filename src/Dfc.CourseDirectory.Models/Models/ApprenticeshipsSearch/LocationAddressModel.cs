using Dfc.CourseDirectory.Models.Interfaces.ApprenticeshipsSearch;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.ApprenticeshipsSearch
{
    public class LocationAddressModel : ILocationAddressModel
    {
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string town { get; set; }
        public string county { get; set; }
        public string postcode { get; set; }
        public double? lat { get; set; }
        public double? @long { get; set; } //need to prefix long with @ because its a reserved word in c#
    }
}
