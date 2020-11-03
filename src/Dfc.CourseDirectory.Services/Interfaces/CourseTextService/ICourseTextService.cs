using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Services.Interfaces.CourseTextService
{
    public interface ICourseTextService
    {
        Task<Result<CourseText>> GetCourseTextByLARS(ICourseTextSearchCriteria criteria);
    }
}
