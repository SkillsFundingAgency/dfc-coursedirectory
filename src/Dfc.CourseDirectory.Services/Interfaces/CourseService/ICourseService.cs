using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Regions;
using Dfc.CourseDirectory.Services.CourseService;

namespace Dfc.CourseDirectory.Services.Interfaces.CourseService
{
    public interface ICourseService
    {
        Task<IResult<Course>> AddCourseAsync(Course course);
        Task<IResult<ICourseSearchResult>> GetYourCoursesByUKPRNAsync(ICourseSearchCriteria criteria);
        Task<IResult<Course>> UpdateCourseAsync(Course course);
        Task<IResult<Course>> GetCourseByIdAsync(IGetCourseByIdCriteria criteria);
        Task<IResult> ArchiveProviderLiveCourses(int? UKPRN);
        Task<IResult> ChangeCourseRunStatusesForUKPRNSelection(int UKPRN, int CurrentStatus, int StatusToBeChangedTo);
        Task<IResult> ArchiveCourseRunsByUKPRN(int UKPRN);
        SelectRegionModel GetRegions();
        Task<IResult<ICourseSearchResult>> GetCoursesByLevelForUKPRNAsync(ICourseSearchCriteria criteria);
        IList<KeyValuePair<string, string>> ValidateCourse(Course course);
        IList<KeyValuePair<string, string>> ValidateCourseRun(CourseRun courseRun, ValidationMode validationMode);
        Task<IResult> UpdateStatus(Guid courseId, Guid courseRunId, int statusToChangeTo);
        IResult<IList<CourseValidationResult>> CourseValidationMessages(IEnumerable<Course> courses, ValidationMode mode);
        Task<IResult<IEnumerable<ICourseStatusCountResult>>> GetCourseCountsByStatusForUKPRN(ICourseSearchCriteria criteria);
        Task<IResult<IEnumerable<Course>>> GetRecentCourseChangesByUKPRN(ICourseSearchCriteria criteria);
        Task<IResult> DeleteBulkUploadCourses(int UKPRN);
        Task<IResult<CourseMigrationReport>> GetCourseMigrationReport(int UKPRN);
        Task<IResult<IList<DfcMigrationReport>>> GetAllDfcReports();
        Task<IResult<int>> GetTotalLiveCourses();
        Task<IResult> ArchiveCoursesExceptBulkUploadReadytoGoLive(int UKPRN, int StatusToBeChangedTo);
    }
}
