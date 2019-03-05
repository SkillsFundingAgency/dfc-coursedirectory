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

        List<BulkUploadCourse> PolulateLARSData(List<BulkUploadCourse> bulkUploadcourses, out List<string> errors);

        List<Course> MappingBulkUploadCourseToCourse(List<BulkUploadCourse> bulkUploadcourses, string userId, out List<string> errors);
    }
}
