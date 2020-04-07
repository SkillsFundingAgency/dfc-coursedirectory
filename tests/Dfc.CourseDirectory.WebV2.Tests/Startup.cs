using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            services.AddRouting();

            services
                .AddAuthentication("Test")
                .AddScheme<TestAuthenticationOptions, TestAuthenticationHandler>("Test", _ => { });

            services.AddCourseDirectory(HostingEnvironment, Configuration);

            services.AddSingleton<TestUserInfo>();
            services.AddSingleton<IDistributedCache, ClearableMemoryCache>();
            services.AddSingleton<IMptxStateProvider, InMemoryMptxStateProvider>();
            services.AddSingleton<IFeatureFlagProvider, ConfigurationFeatureFlagProvider>();
            services.Decorate<IFeatureFlagProvider, OverridableFeatureFlagProvider>();

            DatabaseFixture.ConfigureServices(services);
        }
    }
}
