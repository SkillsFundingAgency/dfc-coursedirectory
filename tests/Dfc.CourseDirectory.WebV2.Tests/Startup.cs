using System;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CosmosDbQueryDispatcher = Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.CosmosDbQueryDispatcher;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class Startup : IStartup
    {
        public Startup(IHostingEnvironment environment, IConfiguration configuration)
        {
            HostingEnvironment = environment;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCourseDirectoryErrorHandling();

            app.UseAuthentication();

            app.UseMvc();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services
                .AddAuthentication("Test")
                .AddScheme<TestAuthenticationOptions, TestAuthenticationHandler>("Test", _ => { });

            services.AddCourseDirectory(HostingEnvironment, Configuration);

            // Make controllers defined in this assembly available
            services.AddMvc().AddApplicationPart(typeof(Startup).Assembly);

            services.AddSingleton<AuthenticatedUserInfo>();
            services.AddSingleton<InMemoryDocumentStore>();
            services.AddTransient<ICosmosDbQueryDispatcher, CosmosDbQueryDispatcher>();
            services.AddSingleton<IMemoryCache, ClearableMemoryCache>();
            services.AddTransient<TestData>();

            services.Scan(scan => scan
                .FromAssembliesOf(typeof(Startup))
                .AddClasses(classes => classes.AssignableTo(typeof(DataStore.CosmosDb.ICosmosDbQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            return services.BuildServiceProvider(validateScopes: true);
        }
    }
}
