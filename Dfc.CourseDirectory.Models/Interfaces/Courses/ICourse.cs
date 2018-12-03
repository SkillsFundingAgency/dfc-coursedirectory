using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Models.Models.Qualifications;

namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface ICourse
    {
        Provider Provider { get; }
        Qualification Qualification { get; }
        CourseData CourseData { get; }
        CourseText CourseText { get; }
    }
}