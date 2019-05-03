using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.ApprenticeshipsSearch
{
    public interface IApprenticeshipLocationModel
    {
        int id { get; set; }
        List<string> deliveryModes { get; set; }
        int? radius { get; set; }
    }
}
