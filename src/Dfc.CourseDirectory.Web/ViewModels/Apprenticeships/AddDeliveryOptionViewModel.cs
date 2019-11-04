using Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships;
using System;

namespace Dfc.CourseDirectory.Web.ViewModels.Apprenticeships
{
    public class AddDeliveryOptionViewModel
    {
        public Guid? LocationId { get; set; }

        public bool DayRelease { get; set; }

        public bool BlockRelease { get; set; }

        public DeliveryOptionSummary DeliveryOptionSummary { get; set; }

        public int? Radius { get; set; }

        public bool? National { get; set; }
    }
}
