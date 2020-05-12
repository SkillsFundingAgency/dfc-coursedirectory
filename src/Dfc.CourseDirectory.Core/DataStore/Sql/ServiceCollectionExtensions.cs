using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Core.DataStore.Sql
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlDataStore(
            this IServiceCollection services,
            Func<IServiceProvider, string> getConnectionString)
        {
            services.Scan(scan => scan
                .FromAssembliesOf(typeof(ISqlQuery<>))
                .AddClasses(classes => classes.AssignableTo(typeof(ISqlQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            services.AddScoped<ISqlQueryDispatcher, SqlQueryDispatcher>();
            services.AddScoped<SqlConnection>(sp => new SqlConnection(getConnectionString(sp)));
            services.AddScoped<SqlTransaction>(sp =>
            {
                var connection = sp.GetRequiredService<SqlConnection>();
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                var transaction = connection.BeginTransaction(IsolationLevel.Snapshot);

                var marker = sp.GetRequiredService<SqlTransactionMarker>();
                marker.OnTransactionCreated(transaction);

                return transaction;
            });
            services.AddScoped<SqlTransactionMarker>();

            return services;
        }
    }
}
