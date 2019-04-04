﻿
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Regions;
using Dfc.CourseDirectory.Services.CourseService;


namespace Dfc.CourseDirectory.Services.Interfaces.CourseService
{
    public interface ICourseService
    {
        Task<IResult<ICourse>> AddCourseAsync(ICourse course);
        Task<IResult<ICourseSearchResult>> GetYourCoursesByUKPRNAsync(ICourseSearchCriteria criteria);

        Task<IResult<ICourse>> UpdateCourseAsync(ICourse course);

        Task<IResult<ICourse>> GetCourseByIdAsync(IGetCourseByIdCriteria criteria);
        Task<IResult> ArchiveProviderLiveCourses(int? UKPRN);
        Task<IResult> ChangeCourseRunStatusesForUKPRNSelection(int UKPRN, int CurrentStatus, int StatusToBeChangedTo);
        SelectRegionModel GetRegions();

        Task<IResult<ICourseSearchResult>> GetCoursesByLevelForUKPRNAsync(ICourseSearchCriteria criteria);

        IList<string> ValidateCourse(ICourse course);

        IList<string> ValidateCourseRun(ICourseRun courseRun, ValidationMode validationMode);

        Task<IResult> UpdateStatus(Guid courseId, Guid courseRunId, int statusToChangeTo);
        IResult<IList<CourseValidationResult>> CourseValidationMessages(IEnumerable<ICourse> courses, ValidationMode mode);
        Task<IResult<IEnumerable<ICourseStatusCountResult>>> GetCourseCountsByStatusForUKPRN(ICourseSearchCriteria criteria);
        Task<IResult<IEnumerable<ICourse>>> GetRecentCourseChangesByUKPRN(ICourseSearchCriteria criteria);
    }
}
