using Dfc.CourseDirectory.Services.Models.Apprenticeships;
using Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships;
using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewModels.Apprenticeships
{
    public class DeliveryOptionsViewModel
    {
        public Guid? LocationId { get; set; }

        public bool DayRelease { get; set; }

        public bool BlockRelease { get; set; }

        public int? Radius { get; set; }

        public bool? National { get; set; }

        public ApprenticeshipMode Mode { get; set; }

        public List<ApprenticeshipLocation> locations { get; set; }

        public bool HasOtherDeliveryOptions { get; set; }


    }
}
