using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships
{
    public class DeliveryOptions
    {
        public bool? SummaryPage { get; set; }
        public List<DeliveryOption> DeliveryOptionItems { get; set; }

        public ApprenticeshipMode Mode { get; set; }


    }
}
