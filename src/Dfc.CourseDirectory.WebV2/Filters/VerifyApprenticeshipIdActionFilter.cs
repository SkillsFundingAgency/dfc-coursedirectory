using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.HttpContextFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class ApprenticeshipIdAttribute : Attribute
    {
        public int DoesNotExistResponseStatusCode { get; set; } = 404;
    }

    public class VerifyApprenticeshipIdActionFilter : IAsyncActionFilter, IOrderedFilter
    {
        public int Order => 0;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var apprenticeshipIdParameters = context.ActionDescriptor.Parameters
                .OfType<ControllerParameterDescriptor>()
                .Where(p => p.ParameterInfo.GetCustomAttribute<ApprenticeshipIdAttribute>() != null)
                .ToList();

            if (apprenticeshipIdParameters.Count > 1)
            {
                throw new InvalidOperationException($"Only a single parameter can be annotated with {nameof(ApprenticeshipIdAttribute)}.");
            }
            else if (apprenticeshipIdParameters.Count == 0)
            {
                await next();
                return;
            }

            var attr = apprenticeshipIdParameters.Single().ParameterInfo.GetCustomAttribute<ApprenticeshipIdAttribute>();

            var apprenticeshipId = context.ActionArguments[apprenticeshipIdParameters.Single().Name];

            if (!(apprenticeshipId is Guid))
            {
                throw new InvalidOperationException($"Apprenticeship ID parameter must be a GUID.");
            }

            var providerOwnershipCache = context.HttpContext.RequestServices.GetRequiredService<IProviderOwnershipCache>();
            var providerContextProvider = context.HttpContext.RequestServices.GetRequiredService<IProviderContextProvider>();

            var providerId = await providerOwnershipCache.GetProviderForApprenticeship((Guid)apprenticeshipId);

            if (!providerId.HasValue)
            {
                context.Result = new StatusCodeResult(attr.DoesNotExistResponseStatusCode);
            }
            else
            {
                // Stash the provider ID so additional filters can use it
                context.HttpContext.Features.Set(new ApprenticeshipProviderFeature(providerId.Value));

                // If the action has a ProviderContext parameter, ensure it's bound and matches this provider ID
                var parameterInfoActionParameters = context.ActionDescriptor.Parameters
                    .Where(p => p.ParameterType == typeof(ProviderContext))
                    .ToList();

                foreach (var p in parameterInfoActionParameters)
                {
                    var boundValue = context.ActionArguments.ContainsKey(p.Name) ? (ProviderContext)context.ActionArguments[p.Name] : null;
                    if (boundValue == null)
                    {
                        var providerInfoCache = context.HttpContext.RequestServices.GetRequiredService<IProviderInfoCache>();
                        var providerInfo = await providerInfoCache.GetProviderInfo(providerId.Value);
                        var providerContext = new ProviderContext(providerInfo);

                        context.ActionArguments[p.Name] = providerContext;
                        providerContextProvider.SetProviderContext(providerContext);
                    }
                    else if (boundValue.ProviderInfo.ProviderId != providerId.Value)
                    {
                        // Bound provider doesn't match this apprenticeship's provider - return an error
                        // (this is either a bug in a redirect or the end user messing with the URL)

                        context.Result = new BadRequestResult();
                        return;
                    }
                }

                await next();
            }
        }
    }
}
