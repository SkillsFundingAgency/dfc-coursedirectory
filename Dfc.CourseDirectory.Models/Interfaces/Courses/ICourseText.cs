using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface ICourseText
    {
        string CourseTitle { get;}
        string Learn { get; }
        string How { get; }
        string Why { get; }
    }
}
