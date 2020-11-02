using System.Collections.Generic;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Services.Interfaces.CourseTextService
{
    public interface ICourseTextSearchResult
    {
        IEnumerable<CourseText> Value { get; }
    }
}
