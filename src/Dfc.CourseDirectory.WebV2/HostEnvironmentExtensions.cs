using Microsoft.Extensions.Hosting;

namespace Dfc.CourseDirectory.WebV2
{
    public static class HostEnvironmentExtensions
    {
        public static bool IsTesting(this IHostEnvironment env) => env.EnvironmentName.Equals("Testing");
    }
}
