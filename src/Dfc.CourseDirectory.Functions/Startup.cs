using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.ReferenceData.Lars;
using Dfc.CourseDirectory.Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Dfc.CourseDirectory.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = GetConfiguration();

            builder.Services.AddSqlDataStore(configuration.GetConnectionString("DefaultConnection"));

            builder.Services.AddTransient<LarsDataImporter>();

            IConfiguration GetConfiguration()
            {
                // Yuk - waiting on https://github.com/Azure/azure-webjobs-sdk/pull/2405 for a nicer way to do this

                var sp = builder.Services.BuildServiceProvider();
                return sp.GetRequiredService<IConfiguration>();
            }
        }
    }
}
