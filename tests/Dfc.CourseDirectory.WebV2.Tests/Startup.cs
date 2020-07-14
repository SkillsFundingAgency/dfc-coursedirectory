using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.WebV2.Cookies;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Dfc.CourseDirectory.WebV2.Tests.ValidationTests;
using FormFlow.State;
using GovUk.Frontend.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class Startup
    {
        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            HostingEnvironment = environment;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostingEnvironment { get; }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCommitSqlTransaction();

            app.UseCourseDirectoryErrorHandling();

            app.UseGdsFrontEnd();

            app.UseSession();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSession();
            services.AddSingleton<ISessionStore, SingletonSessionStore>();

            services.AddRouting();

            services.AddSingleton<IUserInstanceStateStore, TestUserInstanceStateStore>();

            services
                .AddAuthentication("Test")
                .AddScheme<TestAuthenticationOptions, TestAuthenticationHandler>("Test", _ => { });

            services.AddCourseDirectory(HostingEnvironment, Configuration);

            services.AddMediatR(typeof(Startup));
            services.AddBehaviors(typeof(Startup).Assembly);

            services.AddSingleton<TestUserInfo>();
            services.AddSingleton<IDistributedCache, ClearableMemoryCache>();
            services.AddSingleton<IMptxStateProvider, InMemoryMptxStateProvider>();
            services.AddSingleton<IFeatureFlagProvider, ConfigurationFeatureFlagProvider>();
            services.Decorate<IFeatureFlagProvider, OverridableFeatureFlagProvider>();
            services.AddSingleton<Settings>();
            services.AddSingleton<ICookieSettingsProvider, TestCookieSettingsProvider>();
            services.AddTransient<ValidatorBaseTestsValidator>();

            DatabaseFixture.ConfigureServices(services);
        }
    }
}
