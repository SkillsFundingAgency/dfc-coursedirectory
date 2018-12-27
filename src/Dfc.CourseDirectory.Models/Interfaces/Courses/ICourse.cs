using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Models.Models.Qualifications;
using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface ICourse
    {
        Guid id { get; set; }
        QuAP QuAP { get; set; }
        CourseData CourseData { get; set; }
        IEnumerable<CourseRun> CourseRun { get; }
    }
}