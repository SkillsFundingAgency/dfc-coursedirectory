using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Interfaces.Courses;

namespace Dfc.CourseDirectory.Services.Interfaces.CourseTextService
{
    public interface ICourseTextService
    {
        Task<IResult<ICourseText>> GetCourseTextByLARS(ICourseTextSearchCriteria criteria);
    }
}
