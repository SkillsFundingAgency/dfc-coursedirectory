
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

namespace Dfc.CourseDirectory.Web.ViewModels.Locations
{
    public class LocationDeleteViewModel
    {


        public Guid VenueId { get; set; }
        public string VenueName { get; set; }

        public string PostCode { get; set; }

        public string AddressLine1 { get; set; }
        public LocationDelete LocationDelete { get; set; }


    }
}
