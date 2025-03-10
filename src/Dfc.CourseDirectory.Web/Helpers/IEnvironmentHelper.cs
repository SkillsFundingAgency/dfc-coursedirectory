using Dfc.CourseDirectory.Services.Models;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface IEnvironmentHelper
    {
        EnvironmentType GetEnvironmentType();
    }
}
