using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Services.Models.Regions;

namespace Dfc.CourseDirectory.Services.Interfaces.CourseService
{
    public interface ICourseService
    {
        Task<Result<Course>> AddCourseAsync(Course course);
        Task<Result<ICourseSearchResult>> GetYourCoursesByUKPRNAsync(ICourseSearchCriteria criteria);
        Task<Result<Course>> UpdateCourseAsync(Course course);
        Task<Result<Course>> GetCourseByIdAsync(IGetCourseByIdCriteria criteria);
        Task<Result> ArchiveProviderLiveCourses(int? UKPRN);
        Task<Result> ChangeCourseRunStatusesForUKPRNSelection(int UKPRN, int CurrentStatus, int StatusToBeChangedTo);
        Task<Result> ArchiveCourseRunsByUKPRN(int UKPRN);
        SelectRegionModel GetRegions();
        Task<Result<ICourseSearchResult>> GetCoursesByLevelForUKPRNAsync(ICourseSearchCriteria criteria);
        IList<KeyValuePair<string, string>> ValidateCourse(Course course);
        IList<KeyValuePair<string, string>> ValidateCourseRun(CourseRun courseRun, ValidationMode validationMode);
        Task<Result> UpdateStatus(Guid courseId, Guid courseRunId, int statusToChangeTo);
        Result<IList<CourseValidationResult>> CourseValidationMessages(IEnumerable<Course> courses, ValidationMode mode);
        Task<Result<IEnumerable<ICourseStatusCountResult>>> GetCourseCountsByStatusForUKPRN(ICourseSearchCriteria criteria);
        Task<Result<IEnumerable<Course>>> GetRecentCourseChangesByUKPRN(ICourseSearchCriteria criteria);
        Task<Result> DeleteBulkUploadCourses(int UKPRN);
        Task<Result<CourseMigrationReport>> GetCourseMigrationReport(int UKPRN);
        Task<Result<IList<DfcMigrationReport>>> GetAllDfcReports();
        Task<Result<int>> GetTotalLiveCourses();
        Task<Result> ArchiveCoursesExceptBulkUploadReadytoGoLive(int UKPRN, int StatusToBeChangedTo);
    }
}
