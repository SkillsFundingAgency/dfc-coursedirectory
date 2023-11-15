using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.Services
{
    public interface ICourseTypeService
    {
        public Task<Models.CourseType?> GetCourseType(string learnAimRef);
    }
}
