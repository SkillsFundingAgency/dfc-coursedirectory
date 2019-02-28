using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dfc.CourseDirectory.Services.Interfaces.BulkUploadService
{
    public interface IBulkUploadService
    {
        List<string> ProcessBulkUpload(string bulkUploadFilePath, int providerUKPRN, string userId);
    }
}
