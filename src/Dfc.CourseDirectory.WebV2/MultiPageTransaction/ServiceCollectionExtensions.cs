using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMptxInstanceContext(this IServiceCollection services)
        {
            var stateMarkerAttributeType = typeof(IMptxState);
            var stateWithParentMarkerAttributeType = typeof(IMptxState<>);

            var stateTypes = stateMarkerAttributeType.Assembly.GetTypes()
                .Where(t => stateMarkerAttributeType.IsAssignableFrom(t) && !t.IsAbstract && t.IsClass);

            foreach (var t in stateTypes)
            {
                var contextType = typeof(MptxInstanceContext<>).MakeGenericType(t);

                // Register MptxInstanceContext<T> for this T
                services.AddTransient(contextType, sp =>
                {
                    var instanceProvider = sp.GetRequiredService<MptxInstanceProvider>();
                    var instanceContextFactory = sp.GetRequiredService<MptxInstanceContextFactory>();

                    var instance = instanceProvider.GetInstance();
                    return instanceContextFactory.CreateContext(instance, t, parentStateType: null);
                });

                // Does the type implement IMptxState<TParent>?
                var stateWithParent = t.GetInterfaces().Where(
                    t => t.IsGenericType && t.GetGenericTypeDefinition() == stateWithParentMarkerAttributeType)
                    .ToList();

                if (stateWithParent.Count > 1)
                {
                    throw new Exception(
                        "State types may only implement IMptxState<TParent> for a single parent type.");
                }
                else if (stateWithParent.Count == 1)
                {
                    var parentStateType = stateWithParent.Single().GetGenericArguments()[0];
                    var contextWithParentType = typeof(MptxInstanceContext<,>).MakeGenericType(t, parentStateType);

                    services.AddTransient(contextWithParentType, sp =>
                    {
                        var instanceProvider = sp.GetRequiredService<MptxInstanceProvider>();
                        var instanceContextFactory = sp.GetRequiredService<MptxInstanceContextFactory>();

                        var instance = instanceProvider.GetInstance();
                        return instanceContextFactory.CreateContext(instance, t, parentStateType);
                    });
                }
            }

            return services;
        }
    }
}
