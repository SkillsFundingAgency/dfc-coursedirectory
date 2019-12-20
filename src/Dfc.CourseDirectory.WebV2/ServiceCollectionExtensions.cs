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
                    // TODO When the legacy views are all moved this check can go away
                    if (environment.IsTesting())
                    {
                        options.ViewLocationFormats.Clear();
                    }

                    options.ViewLocationFormats.Add("/SharedViews/{0}.cshtml");

                    options.ViewLocationExpanders.Add(new FeatureViewLocationExpander());
                });

            return services;
        }
    }
}
