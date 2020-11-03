
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

namespace Dfc.CourseDirectory.Web.ViewModels.Apprenticeships
{
    public class ConfirmationDeleteViewModel
    {
        public string ApprenticeshipTitle { get; set; }

        public Guid ApprenticeshipId { get; set; }
        public int Level { get; set; }
        public ApprenticeshipDelete ApprenticeshipDelete { get; set; }


    }
}
