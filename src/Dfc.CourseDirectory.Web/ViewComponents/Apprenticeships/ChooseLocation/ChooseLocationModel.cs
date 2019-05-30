using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dfc.CourseDirectory.Models.Models.Courses;
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

        public DeliveryOptionsListModel DeliveryOptionsListItemModel { get; set; }
    }
}
