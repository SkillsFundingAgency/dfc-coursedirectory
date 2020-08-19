using System;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public interface IRequireUserCanAccessCourse<in TRequest>
    {
        Guid GetCourseId(TRequest request);
    }
}
