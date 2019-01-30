using System;
using System.Collections.Generic;
using System.Text;
using Dfc.CourseDirectory.Models.Models.Courses;

namespace Dfc.CourseDirectory.Services.Interfaces.CourseTextService
{
   public interface ICourseTextSearchResult
    {
        IEnumerable<CourseText> Value { get; }
    }
}
