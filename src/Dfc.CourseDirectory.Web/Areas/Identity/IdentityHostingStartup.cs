using System;
using Dfc.CourseDirectory.Areas.Identity.Data;
using Dfc.CourseDirectory.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(Dfc.CourseDirectory.Areas.Identity.IdentityHostingStartup))]
namespace Dfc.CourseDirectory.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<DfcCourseDirectoryContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("DfcUserPocContextConnection")));

                //services.AddDefaultIdentity<DfcCourseDirectoryUser>()
                //    .AddEntityFrameworkStores<DfcUserPocContext>();
            });
        }
    }
}