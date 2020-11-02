using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Services.Models.Regions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseRegion
{
    public class ChooseRegionModel
    {
        public bool UseNationalComponent { get; set; } = true;
        public bool? National { get; set; }
        public SelectRegionModel Regions { get; set; }
        public bool HasOtherDeliveryOptions { get; set; }

    }
}
