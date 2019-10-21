using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewModels.Apprenticeships
{
    public class DeliveryOptionsCombined
    {

        public List<SelectListItem> Locations { get; set; }
        public Guid? LocationId { get; set; }

        public bool DayRelease { get; set; }

        public bool BlockRelease { get; set; }

        public DeliveryOptionSummary DeliveryOptionSummary { get; set; }

        public bool National { get; set; }

        public string Radius { get; set; }

        public ApprenticeshipMode Mode { get; set; }


    }
}
