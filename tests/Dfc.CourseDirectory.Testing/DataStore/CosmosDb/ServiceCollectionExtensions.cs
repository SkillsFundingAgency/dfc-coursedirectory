using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosDbDataStore(this IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssembliesOf(typeof(ServiceCollectionExtensions))
                .AddClasses(classes => classes.AssignableTo(typeof(ICosmosDbQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            services.AddSingleton<ICosmosDbQueryDispatcher>(sp =>
                new Mock<CosmosDbQueryDispatcher>(sp) { CallBase = true }.Object);

            return services;
        }
    }
}
