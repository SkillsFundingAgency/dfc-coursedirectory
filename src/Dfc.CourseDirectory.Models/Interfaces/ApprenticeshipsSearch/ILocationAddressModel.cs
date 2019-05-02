using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.ApprenticeshipsSearch
{
    public interface ILocationAddressModel
    {
        string address1 { get; set; }
        string address2 { get; set; }
        string town { get; set; }
        string county { get; set; }
        string postcode { get; set; }
        double? lat { get; set; }
        double? @long { get; set; } //need to prefix long with @ because its a reserved word in c#
    }
}
