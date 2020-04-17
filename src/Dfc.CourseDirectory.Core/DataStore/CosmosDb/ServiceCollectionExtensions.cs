using System;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosDbDataStore(
            this IServiceCollection services,
            Uri endpoint,
            string key)
        {
            var documentClient = new DocumentClient(endpoint, key);
            services.AddSingleton(documentClient);

            services.AddTransient<ICosmosDbQueryDispatcher, CosmosDbQueryDispatcher>();
            services.AddSingleton<Configuration>();

            services.Scan(scan => scan
                .FromAssembliesOf(typeof(ICosmosDbQuery<>))
                .AddClasses(classes => classes.AssignableTo(typeof(ICosmosDbQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            return services;
        }
    }
}
