using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.DAS;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.DAS
{
    public class DasLocationRef : IDasLocationRef
    {
        public int Id { get; set; }
        public List<string> DeliveryModes { get; set; }
        public int Radius { get; set; }

    }
}
