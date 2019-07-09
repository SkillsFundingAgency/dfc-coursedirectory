
using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.ViewModels.YourCourses;
using System.ComponentModel.DataAnnotations;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Web.ViewModels.Apprenticeships
{
    public class SummaryViewModel
    {
       
        public LocationChoiceSelectionViewModel LocationChoiceSelectionViewModel { get; set; }

        public DetailViewModel DetailViewModel { get; set; }

        public DeliveryViewModel DeliveryViewModel { get; set; }

        public RegionsViewModel RegionsViewModel { get; set; }

        public DeliveryOptionsViewModel DeliveryOptionsViewModel { get; set; }

        public DeliveryOptionsCombinedViewModel DeliveryOptionsCombinedViewModel { get; set; }

        public string[] Regions { get; set; }

        public ApprenticeshipMode  Mode { get; set; }




}
}
