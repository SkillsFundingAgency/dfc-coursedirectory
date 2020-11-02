
using Dfc.CourseDirectory.Services.Models.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.ViewModels.YourCourses;
using System.ComponentModel.DataAnnotations;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Web.ViewModels.Apprenticeships
{
    public class DeliveryViewModel
    {

        public ApprenticeshipDelivery ApprenticeshipDelivery { get; set; }
    }
}
