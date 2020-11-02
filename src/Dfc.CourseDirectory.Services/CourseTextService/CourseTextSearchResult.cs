using System.Collections.Generic;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.Interfaces.CourseTextService;

namespace Dfc.CourseDirectory.Services.CourseTextService
{
    public class CourseTextSearchResult : ICourseTextSearchResult
    {
        public IEnumerable<CourseText> Value { get; set; }

        public CourseTextSearchResult(
        IEnumerable<CourseText> value)
        {
            Throw.IfNull(value, nameof(value));
            Value = value;
        }
    }
}
