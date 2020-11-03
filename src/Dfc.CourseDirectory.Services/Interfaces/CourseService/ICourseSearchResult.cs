using System.Collections.Generic;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Services.Interfaces.CourseService
{
    public interface ICourseSearchResult
    {
        IEnumerable<ICourseSearchOuterGrouping> Value { get; set; }
    }

    public interface ICourseSearchOuterGrouping
    {
        string QualType { get; }
        string Level { get; }
        IEnumerable<ICourseSearchInnerGrouping> Value { get; set; }
    }

    public interface ICourseSearchInnerGrouping
    {
        string LARSRef { get; }
        IEnumerable<Course> Value { get; set; }
    }
}
