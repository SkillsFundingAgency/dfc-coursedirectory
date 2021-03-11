using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Web.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Web.ViewModels.Apprenticeships
{
    public class DeliveryOptionsCombined
    {

        public List<ApprenticeshipLocation> Locations { get; set; }
        public Guid? LocationId { get; set; }

        public bool DayRelease { get; set; }

        public bool BlockRelease { get; set; }

        public bool National { get; set; }

        public string Radius { get; set; }

        public ApprenticeshipMode Mode { get; set; }

        public bool HasOtherDeliveryOptions { get; set; }


    }
}
