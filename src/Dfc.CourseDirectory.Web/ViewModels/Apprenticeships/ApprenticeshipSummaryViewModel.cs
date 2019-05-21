
using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.ViewModels.YourCourses;
using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Web.ViewModels.Apprenticeships
{
    public class ApprenticeshipSummaryViewModel
    {
       
        public ApprenticeshipLocationChoiceSelectionViewModel ApprenticeshipLocationChoiceSelectionViewModel { get; set; }

        public ApprenticeshipDetailViewModel ApprenticeshipDetailViewModel { get; set; }

        public ApprenticeshipDeliveryViewModel ApprenticeshipDeliveryViewModel { get; set; }

        public ApprenticeshipRegionsViewModel ApprenticeshipRegionsViewModel { get; set; }

        public ApprenticeshipDeliveryOptionsViewModel ApprenticeshipDeliveryOptionsViewModel { get; set; }






    }
}
