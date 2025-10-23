using Dfc.CourseDirectory.Services.Models;

namespace Dfc.CourseDirectory.WebV2.Helpers
{
    public interface IEnvironmentHelper
    {
        EnvironmentType GetEnvironmentType();
    }
}
