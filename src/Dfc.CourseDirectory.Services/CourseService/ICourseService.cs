using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Services.Models.Regions;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public interface ICourseService
    {
        Task<Result<Course>> AddCourseAsync(Course course);
        Task<Result<CourseSearchResult>> GetYourCoursesByUKPRNAsync(CourseSearchCriteria criteria);
        Task<Result<Course>> UpdateCourseAsync(Course course);
        Task<Result<Course>> GetCourseByIdAsync(GetCourseByIdCriteria criteria);
        Task<Result> ChangeCourseRunStatusesForUKPRNSelection(int UKPRN, int CurrentStatus, int StatusToBeChangedTo);
        Task<Result> ArchiveCourseRunsByUKPRN(int UKPRN);
        SelectRegionModel GetRegions();
        Task<Result<CourseSearchResult>> GetCoursesByLevelForUKPRNAsync(CourseSearchCriteria criteria);
        IList<KeyValuePair<string, string>> ValidateCourse(Course course);
        IList<KeyValuePair<string, string>> ValidateCourseRun(CourseRun courseRun, ValidationMode validationMode);
        Task<Result> UpdateStatus(Guid courseId, Guid courseRunId, int statusToChangeTo);
        Result<IList<CourseValidationResult>> CourseValidationMessages(IEnumerable<Course> courses, ValidationMode mode);
        Task<Result<IEnumerable<CourseStatusCountResult>>> GetCourseCountsByStatusForUKPRN(CourseSearchCriteria criteria);
        Task<Result> DeleteBulkUploadCourses(int UKPRN);
        Task<Result<CourseMigrationReport>> GetCourseMigrationReport(int UKPRN);
        Task<Result> ArchiveCoursesExceptBulkUploadReadytoGoLive(int UKPRN, int StatusToBeChangedTo);
    }
}
