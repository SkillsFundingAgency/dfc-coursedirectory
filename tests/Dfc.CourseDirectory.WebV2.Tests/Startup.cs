using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class Startup : IStartup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddCourseDirectory();

            return services.BuildServiceProvider(validateScopes: true);
        }
    }
}
