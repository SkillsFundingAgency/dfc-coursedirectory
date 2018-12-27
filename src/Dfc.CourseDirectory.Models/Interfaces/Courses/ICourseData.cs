using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface ICourseData
    {
        Guid ID { get; set; }
        Guid CourseID { get; set; }
        string CourseTitle { get; set; }
    }
}
