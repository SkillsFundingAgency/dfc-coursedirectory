using Dfc.CourseDirectory.Models.Interfaces.Courses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public class BulkUploadError : IBulkUploadError
    {
        public int LineNumber { get; set; }
        public string Header { get; set; }
        public string Error { get; set; }
    }
}
