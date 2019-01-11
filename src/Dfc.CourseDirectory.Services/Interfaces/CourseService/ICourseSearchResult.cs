
using Dfc.CourseDirectory.Models.Models.Courses;
using System.Collections.Generic;


namespace Dfc.CourseDirectory.Services.Interfaces.CourseService
{
    public interface ICourseSearchResult
    {
        IEnumerable<ICourseSearchOuterGrouping> Value { get; }
    }

    public interface ICourseSearchOuterGrouping
    {
        string QualType { get; }
        IEnumerable<ICourseSearchInnerGrouping> Value { get; }
    }

    public interface ICourseSearchInnerGrouping
    {
        string LARSRef { get; }
        IEnumerable<Course> Value { get; }
    }
}
