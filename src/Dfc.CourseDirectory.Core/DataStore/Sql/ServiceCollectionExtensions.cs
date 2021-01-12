using System.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Core.DataStore.Sql
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlDataStore(
            this IServiceCollection services,
            string connectionString)
        {
            services.Scan(scan => scan
                .FromAssembliesOf(typeof(ISqlQuery<>))
                .AddClasses(classes => classes.AssignableTo(typeof(ISqlQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            services.AddScoped<ISqlQueryDispatcher, SqlQueryDispatcher>();
            services.AddScoped<SqlConnection>(_ => new SqlConnection(connectionString));
            services.AddScoped<SqlTransactionMarker>();

            return services;
        }
    }
}
