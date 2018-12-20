using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseServiceSettings : ICourseServiceSettings
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
    }
}
