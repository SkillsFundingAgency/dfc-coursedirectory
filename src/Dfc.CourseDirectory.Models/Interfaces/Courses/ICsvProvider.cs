using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface ICsvProvider
    {
        string ProviderUKPRN { get; set; }
        string ProviderName { get; set; }
    }
}
