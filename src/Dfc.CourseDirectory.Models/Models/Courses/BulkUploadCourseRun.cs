using Dfc.CourseDirectory.Models.Interfaces.Courses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public class BulkUploadCourseRun : IBulkUploadCourseRun
    {
        public string LearnAimRef { get; set; }
        public int TempCourseId { get; set; }
        public CourseRun CourseRun { get; set; }
    }
}
