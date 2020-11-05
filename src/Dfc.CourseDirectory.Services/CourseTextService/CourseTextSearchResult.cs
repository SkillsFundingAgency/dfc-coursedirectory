using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Services.CourseTextService
{
    public class CourseTextSearchResult
    {
        public IEnumerable<CourseText> Value { get; set; }

        public CourseTextSearchResult(
        IEnumerable<CourseText> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Value = value;
        }
    }
}
