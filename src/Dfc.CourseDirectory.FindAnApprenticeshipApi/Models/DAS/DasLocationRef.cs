using Dfc.Providerportal.FindAnApprenticeship.Interfaces.DAS;
using System;
using System.Collections.Generic;

namespace Dfc.Providerportal.FindAnApprenticeship.Models.DAS
{
    public class DasLocationRef : IDasLocationRef
    {
        public int Id { get; set; }
        public List<string> DeliveryModes { get; set; }
        public int Radius { get; set; }

    }
}
