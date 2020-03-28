using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using CosmosDbQueryDispatcher = Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.CosmosDbQueryDispatcher;

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
            services.AddSingleton<InMemoryDocumentStore>();
            services.AddSingleton<ICosmosDbQueryDispatcher>(sp => new Mock<CosmosDbQueryDispatcher>(sp) { CallBase = true }.Object);
            services.AddSingleton<IMemoryCache, ClearableMemoryCache>();
            services.AddTransient<TestData>();
            services.AddSingleton<IClock, MutableClock>();
            services.AddSingleton<IMptxStateProvider, InMemoryMptxStateProvider>();
            services.AddSingleton<SqlQuerySpy>();
            services.Decorate<ISqlQueryDispatcher, SqlQuerySpyDecorator>();

            services.Scan(scan => scan
                .FromAssembliesOf(typeof(Startup))
                .AddClasses(classes => classes.AssignableTo(typeof(DataStore.CosmosDb.ICosmosDbQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());
        }
    }
}
