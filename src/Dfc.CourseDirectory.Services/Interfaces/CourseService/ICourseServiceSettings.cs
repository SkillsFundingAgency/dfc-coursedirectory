using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.Interfaces.CourseService
{
    public interface ICourseServiceSettings
    {
        string ApiUrl { get; set; }
        string ApiKey { get; set; }
    }
}
