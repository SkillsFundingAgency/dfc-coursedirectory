﻿
using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.ViewModels.YourCourses;
using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Web.ViewModels.ProviderApprenticeships
{
    public class ProviderApprenticeshipsViewModel
    {
        [RegularExpression(@"[a-zA-Z0-9 \¬\!\£\$\%\^\&\*\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`\" + "\"" + @"\\]+", ErrorMessage = "Search contains invalid characters")]
        public string Search { get; set; }
       


    }
}
