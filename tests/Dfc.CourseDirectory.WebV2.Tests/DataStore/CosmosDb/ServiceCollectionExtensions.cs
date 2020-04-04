using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosDbDataStore(this IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssembliesOf(typeof(Startup))
                .AddClasses(classes => classes.AssignableTo(typeof(ICosmosDbQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            services.AddSingleton<ICosmosDbQueryDispatcher>(sp =>
                new Mock<CosmosDbQueryDispatcher>(sp) { CallBase = true }.Object);

            return services;
        }
    }
}
