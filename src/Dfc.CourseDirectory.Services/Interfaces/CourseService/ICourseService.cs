
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models;


namespace Dfc.CourseDirectory.Services.Interfaces.CourseService
{
    public interface ICourseService
    {
        Task<IResult<ICourse>> AddCourseAsync(ICourse course);
        Task<IResult<ICourseSearchResult>> GetYourCoursesByUKPRNAsync(ICourseSearchCriteria criteria);

        Task<IResult<ICourse>> UpdateCourseAsync(ICourse course);

        Task<IResult<ICourse>> GetCourseByIdAsync(IGetCourseByIdCriteria criteria);

        SelectRegionModel GetRegions();

    }
}
