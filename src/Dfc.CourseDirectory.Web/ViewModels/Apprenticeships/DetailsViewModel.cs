
using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.ViewModels.YourCourses;
using System.ComponentModel.DataAnnotations;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Web.ViewModels.Apprenticeships
{
    public class DetailViewModel
    {

        public Guid Id { get; set; }
        public int? StandardCode { get; set; }
        public int? FrameworkCode { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public int? ProgType { get; set; }
        public string NotionalEndLevel { get; set; }
        public int? Version { get; set; }
        public int? PathwayCode { get; set; }
        public ApprenticeshipType ApprenticeshipType { get; set; }
        public ApprenticeShipPreviousPage ApprenticeshipPreviousPage { get; set; }
        public string Information { get; set; }

        public string Website { get; set; }

        public string Email { get; set; }

        public string Telephone { get; set; }

        public string ContactUsIUrl { get; set; }

        public string Mode { get; set; }



    }
}
