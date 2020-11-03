using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Services.CourseTextService
{
    public interface ICourseTextService
    {
        Task<Result<CourseText>> GetCourseTextByLARS(CourseTextSearchCriteria criteria);
    }
}
