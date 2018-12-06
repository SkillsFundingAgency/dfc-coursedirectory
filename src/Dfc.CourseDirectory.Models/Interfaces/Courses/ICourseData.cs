using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface ICourseData
    {
        Guid ID { get; }
        Guid CourseID { get; }
        string CourseTitle { get; }
    }
}
