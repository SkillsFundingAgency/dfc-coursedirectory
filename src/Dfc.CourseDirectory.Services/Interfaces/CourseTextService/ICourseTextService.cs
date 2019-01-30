using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Services.Interfaces.CourseTextService
{
    public interface ICourseTextService
    {
        Task<IResult<IEnumerable<ICourseTextSearchResult>>> GetCourseTextByLARS(ICourseTextSearchCriteria criteria, string LARSRef);
    }
}
