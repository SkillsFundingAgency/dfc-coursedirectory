using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Models.Models.Qualifications;
using Dfc.CourseDirectory.Models.Models.Venues;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface ICourse
    {
        Provider Provider { get; }
        Qualification Qualification { get; }
        Venue Venue { get; }

    }
}
