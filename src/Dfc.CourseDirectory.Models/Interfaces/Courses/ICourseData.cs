using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Venues;

namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface ICourseData
    {
        Venue Venue { get; }
        CourseInformation Information { get; }
    }
}