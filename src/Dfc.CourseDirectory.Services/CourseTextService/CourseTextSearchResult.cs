using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.CourseTextService;
using System;
using System.Collections.Generic;
using System.Text;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Venues;

namespace Dfc.CourseDirectory.Services.CourseTextService
{
    public class CourseTextSearchResult : ValueObject<CourseTextSearchResult>, ICourseTextSearchResult
    {
        public IEnumerable<CourseText> Value { get; set; }

        public CourseTextSearchResult(
        IEnumerable<CourseText> value)
        {
            Throw.IfNull(value, nameof(value));
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

    }
}
