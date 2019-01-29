using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.Interfaces.CourseTextService
{
    public interface ICourseTextServiceSettings
    {
        string ApiUrl { get; set; }
        string ApiKey { get; set; }
    }
}
