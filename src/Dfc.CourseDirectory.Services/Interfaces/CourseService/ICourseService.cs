
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models;
using Dfc.CourseDirectory.Models.Models.Courses;

namespace Dfc.CourseDirectory.Services.Interfaces.CourseService
{
    public interface ICourseService
    {
        Task<IResult<ICourse>> AddCourseAsync(ICourse course);
        Task<IResult<ICourseSearchResult>> GetYourCoursesByUKPRNAsync(ICourseSearchCriteria criteria);

        Task<IResult<ICourse>> UpdateCourseAsync(ICourse course);

        Task<IResult<ICourse>> GetCourseByIdAsync(IGetCourseByIdCriteria criteria);
        Task<IResult> ArchiveProviderLiveCourses(int? UKPRN);
        SelectRegionModel GetRegions();

        Task<IResult<ICourseSearchResult>> GetCoursesByLevelForUKPRNAsync(ICourseSearchCriteria criteria);

        IList<string> ValidateCourse(ICourse course);

        IList<string> ValidateCourseRun(ICourseRun courseRun, ValidationMode validationMode);

        Task<IResult> UpdateStatus(Guid courseId, Guid courseRunId, int statusToChangeTo);
    }
}
