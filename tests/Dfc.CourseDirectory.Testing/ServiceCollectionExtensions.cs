using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Testing
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInterceptor<T>(this IServiceCollection services)
            where T : class
        {
            return services
                .AddSingleton<Interceptor<T>>()
                .Decorate<T>((inner, serviceProvider) =>
                {
                    var interceptor = serviceProvider.GetRequiredService<Interceptor<T>>();
                    return interceptor.CreateProxy(inner);
                });
        }
    }
}
