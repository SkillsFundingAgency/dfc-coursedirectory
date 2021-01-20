using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Models
{
    public interface IBulkUploadError
    {
        int LineNumber { get; }
        string Header { get; }
        string Error { get; }
    }
}
