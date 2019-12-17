using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCourseDirectory(this IServiceCollection services)
        {
            return services;
        }
    }
}
