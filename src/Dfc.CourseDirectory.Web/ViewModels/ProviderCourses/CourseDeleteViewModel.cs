
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

namespace Dfc.CourseDirectory.Web.ViewModels.ProviderCourses
{
    public class CourseDeleteViewModel
    {


        public Guid CourseId { get; set; }

        public Guid CourseRunId { get; set; }
        public string CourseName { get; set; }

        public CourseDelete CourseDelete { get; set; }


    }
}
