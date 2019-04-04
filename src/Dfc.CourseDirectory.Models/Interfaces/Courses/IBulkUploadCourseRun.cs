using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface IBulkUploadCourseRun
    {
        string LearnAimRef { get; set; }
        int TempCourseId { get; set; }
        CourseRun CourseRun { get; set; }
    }
}
