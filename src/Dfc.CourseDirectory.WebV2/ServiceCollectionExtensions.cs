using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Dfc.CourseDirectory.WebV2
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCourseDirectory(this IServiceCollection services)
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
                    options.FileProviders.Add(new ManifestEmbeddedFileProvider(thisAssembly));
                    options.ViewLocationExpanders.Add(new FeatureViewLocationExpander());
                });

            return services;
        }
    }
}
