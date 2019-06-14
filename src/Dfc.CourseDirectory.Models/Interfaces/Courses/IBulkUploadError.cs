using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface IBulkUploadError
    {
        int LineNumber { get; }
        string Header { get; }
        string Error { get; }
    }
}
