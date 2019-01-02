
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Interfaces.Courses;


namespace Dfc.CourseDirectory.Services.Interfaces.CourseService
{
    public interface ICourseService
    {
        Task<IResult<ICourse>> AddCourseAsync(ICourse course);
    }
}
