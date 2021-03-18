using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Services.BulkUploadService
{
    public interface IBulkUploadService
    {
        Task<List<string>> ProcessBulkUpload(Stream stream, int providerUKPRN, string userId, bool uploadCourses);        
        List<BulkUploadCourse> PolulateLARSData(List<BulkUploadCourse> bulkUploadcourses, out List<string> errors);
        List<Course> MappingBulkUploadCourseToCourse(List<BulkUploadCourse> bulkUploadcourses, string userId, out List<string> errors);
        int CountCsvLines(Stream stream);
        int BulkUploadSecondsPerRecord { get; }
    }
}
