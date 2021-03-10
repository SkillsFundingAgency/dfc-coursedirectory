using System;
using Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships;

namespace Dfc.CourseDirectory.Web.ViewModels.Apprenticeships
{
    public class AddDeliveryOptionsCombinedModel
    {
        public Guid? LocationId { get; set; }

        public bool DayRelease { get; set; }

        public bool BlockRelease { get; set; }

        public DeliveryOptionSummary DeliveryOptionSummary { get; set; }

        public bool National { get; set; }

        public string Radius { get; set; }
    }
}
