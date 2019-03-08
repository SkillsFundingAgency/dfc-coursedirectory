
using Dfc.CourseDirectory.Models.Models.Courses;
using System.Collections.Generic;


namespace Dfc.CourseDirectory.Services.Interfaces.CourseService
{
    public interface ICourseStatusCountResult
    {
        int Status { get; set; }
        string Description { get; set; }
        int Count { get; set; }
    }
}
