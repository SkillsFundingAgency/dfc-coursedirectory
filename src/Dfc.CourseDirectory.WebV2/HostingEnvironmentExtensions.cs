using Microsoft.AspNetCore.Hosting;

namespace Dfc.CourseDirectory.WebV2
{
    public static class HostingEnvironmentExtensions
    {
        public static bool IsTesting(this IHostingEnvironment env) => env.EnvironmentName.Equals("Testing");
    }
}
