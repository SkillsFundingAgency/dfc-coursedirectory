using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Services.Interfaces.CourseTextService
{
    public interface ICourseTextService
    {
        Task<IResult<CourseText>> GetCourseTextByLARS(ICourseTextSearchCriteria criteria);
    }
}
