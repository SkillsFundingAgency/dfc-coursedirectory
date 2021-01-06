using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.DAS
{
    public interface IDasLocationRef
    {
        List<string> DeliveryModes { get; set; }
        int Id { get; set; }
        int Radius { get; set; }
    }
}
