using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dfc.CourseDirectory.Models.Models;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Regions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseRegion
{
    public class ChooseRegionModel
    {
        public bool UseNationalComponent { get; set; } = true;
        public bool? National { get; set; }
        public SelectRegionModel Regions { get; set; }

      
    }
}
