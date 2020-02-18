using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.Filters;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dfc.CourseDirectory.WebV2
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCourseDirectory(
            this IServiceCollection services,
            IHostingEnvironment environment)
        {
            var thisAssembly = typeof(ServiceCollectionExtensions).Assembly;

            if (!environment.IsTesting())
            {
                services.AddTransient<ICosmosDbQueryDispatcher, CosmosDbQueryDispatcher>();
                services.AddSingleton<Configuration>();

                services.Scan(scan => scan
                    .FromAssembliesOf(typeof(ICosmosDbQuery<>))
                    .AddClasses(classes => classes.AssignableTo(typeof(ICosmosDbQueryHandler<,>)))
                        .AsImplementedInterfaces()
                        .WithTransientLifetime());
            }
            
            services
                .AddMvc(options =>
                {
                    options.Conventions.Add(new AddFeaturePropertyModelConvention());

                    options.Filters.Add(new RedirectToProviderSelectionActionFilter());
                    options.Filters.Add(new VerifyApprenticeshipIdActionFilter());

                    options.ModelBinderProviders.Insert(0, new CurrentProviderModelBinderProvider());
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

            services.AddSingleton<HostingOptions>();
            services.AddSingleton<IProviderOwnershipCache, ProviderOwnershipCache>();
            services.AddSingleton<IProviderInfoCache, ProviderInfoCache>();
            services.AddGovUkFrontend();

            return services;
        }
    }
}
