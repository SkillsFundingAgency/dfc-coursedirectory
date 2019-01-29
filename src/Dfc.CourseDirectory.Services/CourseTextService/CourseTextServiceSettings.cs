using Dfc.CourseDirectory.Services.Interfaces.CourseTextService;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.CourseTextService
{
    public class CourseTextServiceSettings : ICourseTextServiceSettings
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
    }
}
