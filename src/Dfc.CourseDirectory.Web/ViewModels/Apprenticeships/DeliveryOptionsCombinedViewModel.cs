﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships;

namespace Dfc.CourseDirectory.Web.ViewModels.Apprenticeships
{
    public class DeliveryOptionsCombinedViewModel
    {

        public List<SelectListItem> Locations { get; set; }
        public Guid? LocationId { get; set; }

        public bool DayRelease { get; set; }

        public bool BlockRelease { get; set; }

        public DeliveryOptions DeliveryOptions { get; set; }

        public bool National { get; set; }

        public string Radius { get; set; }

        public ApprenticeshipMode Mode { get; set; }


    }
}
