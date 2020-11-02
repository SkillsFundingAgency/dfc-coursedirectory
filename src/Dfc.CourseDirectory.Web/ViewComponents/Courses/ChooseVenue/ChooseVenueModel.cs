using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dfc.CourseDirectory.Services.Models.Courses;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseVenue
{
    public class ChooseVenueModel
    {
        public Guid? VenueId { get; set; }
        public List<SelectListItem> Venues { get; set; }
      
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }

        public bool DisplayLink { get; set; }
    }
}
