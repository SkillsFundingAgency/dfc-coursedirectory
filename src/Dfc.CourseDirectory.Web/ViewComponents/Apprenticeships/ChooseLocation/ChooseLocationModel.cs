using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.Models.Apprenticeships;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships
{
    public class ChooseLocationModel
    {
        public Guid? LocationId { get; set; }
        public List<SelectListItem> Locations { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public bool DisplayLink { get; set; }
        public List<ApprenticeshipLocation> DeliveryLocations { get; set; }
        public ApprenticeshipLocationType Type { get; set; }
    }
}
