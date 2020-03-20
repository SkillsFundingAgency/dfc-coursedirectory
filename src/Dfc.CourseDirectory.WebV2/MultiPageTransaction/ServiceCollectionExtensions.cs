﻿using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMptxInstanceContext(this IServiceCollection services)
        {
            var markerAttributeType = typeof(IMptxState);

            var stateTypes = markerAttributeType.Assembly.GetTypes()
                .Where(t => markerAttributeType.IsAssignableFrom(t) && !t.IsAbstract && t.IsClass);

            foreach (var t in stateTypes)
            {
                var contextType = typeof(MptxInstanceContext<>).MakeGenericType(t);

                services.AddTransient(contextType, sp =>
                {
                    var contextProvider = sp.GetRequiredService<MptxInstanceContextProvider>();
                    var method = contextProvider.GetType().GetMethod(nameof(MptxInstanceContextProvider.GetContext)).MakeGenericMethod(t);
                    return method.Invoke(contextProvider, new object[0]);
                });
            }

            return services;
        }
    }
}
