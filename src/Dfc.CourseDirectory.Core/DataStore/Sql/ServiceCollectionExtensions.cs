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
                    .WithTransientLifetime()
                .AddClasses(classes => classes.AssignableTo(typeof(ISqlAsyncEnumerableQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            services.AddTransient<SqlConnection>(_ => new SqlConnection(connectionString));
            services.AddSingleton<ISqlQueryDispatcherFactory, ServiceProviderSqlDispatcherFactory>();
            services.AddScoped<ISqlQueryDispatcher>(serviceProvider =>
            {
                var factory = serviceProvider.GetRequiredService<ISqlQueryDispatcherFactory>();
                var scopeMarker = serviceProvider.GetRequiredService<SqlTransactionMarker>();

                var dispatcher = factory.CreateDispatcher();
                scopeMarker.OnTransactionCreated(dispatcher.Transaction);

                return dispatcher;
            });
            services.AddScoped<SqlTransactionMarker>();

            return services;
        }
    }
}
