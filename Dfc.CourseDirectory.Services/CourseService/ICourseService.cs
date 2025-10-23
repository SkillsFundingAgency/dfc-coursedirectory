using Dfc.CourseDirectory.Services.Models.Regions;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public interface ICourseService
    {
        SelectRegionModel GetRegions();
    }
}
