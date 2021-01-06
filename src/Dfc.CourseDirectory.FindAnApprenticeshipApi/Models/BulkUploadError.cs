using System;
using System.Collections.Generic;
using System.Text;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Models;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Models
{
    public class BulkUploadError : IBulkUploadError
    {
        public int LineNumber { get; set; }
        public string Header { get; set; }
        public string Error { get; set; }
    }
}
