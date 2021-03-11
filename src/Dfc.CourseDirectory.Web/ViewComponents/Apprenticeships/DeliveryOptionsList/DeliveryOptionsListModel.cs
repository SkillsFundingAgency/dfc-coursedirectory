using System.Collections.Generic;
using Dfc.CourseDirectory.Web.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships
{
    public class DeliveryOptionSummary
    {
        public bool? SummaryPage { get; set; }
        public List<DeliveryOption> DeliveryOptions { get; set; }

        public ApprenticeshipMode Mode { get; set; }


    }
}
