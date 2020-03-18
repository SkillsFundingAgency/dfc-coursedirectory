using System;
using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBehaviors(this IServiceCollection services)
        {
            foreach (var type in typeof(ServiceCollectionExtensions).Assembly.GetTypes())
            {
                RegisterRequireUserCanSubmitQASubmissionBehavior(type);
                RegisterRestrictProviderTypeBehavior(type);
                RegisterRestrictQAStatusBehavior(type);
            }

            return services;

            void RegisterRequireUserCanSubmitQASubmissionBehavior(Type type)
            {
                // For any type implementing IRequireUserCanSubmitQASubmission<TRequest, TResponse>
                // add a RequireUserCanSubmitQASubmissionBehavior behavior for its request & response types

                var requireCanSubmitQASubmissionType = typeof(IRequireUserCanSubmitQASubmission<>);

                var restrictTypes = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == requireCanSubmitQASubmissionType)
                    .ToList();

                foreach (var t in restrictTypes)
                {
                    var requestType = t.GenericTypeArguments[0];
                    var responseType = GetResponseTypeFromRequestType(requestType);

                    var pipelineBehaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
                    var behaviourType = typeof(RequireUserCanSubmitQASubmissionBehavior<,>).MakeGenericType(requestType, responseType);
                    services.AddScoped(pipelineBehaviorType, behaviourType);

                    // Register the handler as an IRequireUserCanSubmitQASubmission<TRequest, TResponse>
                    var handlerInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
                    services.AddScoped(t, sp => sp.GetRequiredService(handlerInterfaceType));
                }
            }

            void RegisterRestrictProviderTypeBehavior(Type type)
            {
                // For any type implementing IRestrictProviderType<TRequest, TResponse>
                // add a RestrictQAStatusBehavior behavior for its request & response types

                var restrictProviderTypeType = typeof(IRestrictProviderType<>);

                var restrictTypes = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == restrictProviderTypeType)
                    .ToList();

                foreach (var t in restrictTypes)
                {
                    var requestType = t.GenericTypeArguments[0];
                    var responseType = GetResponseTypeFromRequestType(requestType);

                    var pipelineBehaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
                    var behaviourType = typeof(RestrictProviderTypeBehavior<,>).MakeGenericType(requestType, responseType);
                    services.AddScoped(pipelineBehaviorType, behaviourType);

                    // Register the handler as an IRestrictProviderType<TRequest, TResponse>
                    var handlerInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
                    services.AddScoped(t, sp => sp.GetRequiredService(handlerInterfaceType));
                }
            }

            void RegisterRestrictQAStatusBehavior(Type type)
            { 
                // For any type implementing IRestrictQAStatus<TRequest, TResponse>
                // add a RestrictQAStatusBehavior behavior for its request & response types

                var restrictQaStatusType = typeof(IRestrictQAStatus<>);

                var restrictTypes = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == restrictQaStatusType)
                    .ToList();

                foreach (var t in restrictTypes)
                {
                    var requestType = t.GenericTypeArguments[0];
                    var responseType = GetResponseTypeFromRequestType(requestType);

                    var pipelineBehaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
                    var behaviourType = typeof(RestrictQAStatusBehavior<,>).MakeGenericType(requestType, responseType);
                    services.AddScoped(pipelineBehaviorType, behaviourType);

                    // Register the handler as an IRestrictQAStatus<TRequest, TResponse>
                    var handlerInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
                    services.AddScoped(t, sp => sp.GetRequiredService(handlerInterfaceType));
                }
            }

            Type GetResponseTypeFromRequestType(Type requestType) =>
                requestType
                    .GetInterfaces()
                    .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                    .GetGenericArguments()[0];
        }
    }
}
