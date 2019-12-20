using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCourseDirectory(
            this IServiceCollection services,
            IHostingEnvironment environment)
        {
            var thisAssembly = typeof(ServiceCollectionExtensions).Assembly;
            
            services
                .AddMvc(options =>
                {
                    options.Conventions.Add(new AddFeaturePropertyModelConvention());
                })
                .AddApplicationPart(thisAssembly)
                .AddRazorOptions(options =>
                {
                    options.ViewLocationExpanders.Add(new FeatureViewLocationExpander());
                });

            return services;
        }
    }
}
