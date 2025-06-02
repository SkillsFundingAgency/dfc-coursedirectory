using System;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.BackgroundWorkers;
using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.Core.Services;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.Web.Tests.Core;
using Dfc.CourseDirectory.Web.Tests.Data;
using Dfc.CourseDirectory.WebV2;
using Dfc.CourseDirectory.WebV2.Cookies;
using Dfc.CourseDirectory.WebV2.FeatureFlagProviders;
using Dfc.CourseDirectory.WebV2.Features.DataManagement;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using FormFlow.State;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dfc.CourseDirectory.Web.Tests
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

            app.UseSession();

            app.UseRouting();

            app.UseAuthentication();

            app.UseMiddleware<ProviderContextMiddleware>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapV2Hubs();
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSession();
            services.AddSingleton<ISessionStore, SingletonSessionStore>();

            services.AddRouting();
            services.AddSignalR();

            services.AddSingleton<IUserInstanceStateStore, TestUserInstanceStateStore>();

            services
                .AddAuthentication("Test")
                .AddScheme<TestAuthenticationOptions, TestAuthenticationHandler>("Test", _ => { });

            services.AddCourseDirectory(HostingEnvironment, Configuration);

            services.Configure<GoogleWebRiskSettings>(
            Configuration.GetSection(nameof(GoogleWebRiskSettings)));

            var mockWebRiskService = new Mock<IWebRiskService>();
            mockWebRiskService.Setup(x => x.CheckForSecureUri(It.IsAny<string>())).ReturnsAsync(true);
            services.AddScoped<IWebRiskService>(_ => mockWebRiskService.Object);

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(Startup).Assembly));

            services.AddSingleton<TestUserInfo>();
            services.AddSingleton<IDistributedCache, ClearableMemoryCache>();
            services.AddSingleton<IMptxStateProvider, InMemoryMptxStateProvider>();
            services.AddSingleton<IFeatureFlagProvider, ConfigurationFeatureFlagProvider>();
            services.Decorate<IFeatureFlagProvider, DataManagementFeatureFlagProvider>();
            services.Decorate<IFeatureFlagProvider, OverridableFeatureFlagProvider>();
            services.AddSingleton<Settings>();
            services.AddSingleton<ICookieSettingsProvider, TestCookieSettingsProvider>();
            services.AddSingleton<IBackgroundWorkScheduler, ExecuteImmediatelyBackgroundWorkScheduler>();

            services.Configure<DataManagementOptions>(
                options => options.ProcessedImmediatelyThreshold = TimeSpan.FromMilliseconds(2000));

            DatabaseFixture.ConfigureServices(services);
        }
    }
}
