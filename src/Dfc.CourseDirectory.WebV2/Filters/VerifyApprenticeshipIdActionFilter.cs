using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class ApprenticeshipIdAttribute : Attribute
    {
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

            var apprenticeshipId = context.ActionArguments[apprenticeshipIdParameters.Single().Name];

            if (!(apprenticeshipId is Guid))
            {
                throw new InvalidOperationException($"Apprenticeship ID parameter must be a GUID.");
            }

            var providerOwnershipCache = context.HttpContext.RequestServices.GetRequiredService<IProviderOwnershipCache>();
            var ukprn = await providerOwnershipCache.GetProviderForApprenticeship((Guid)apprenticeshipId);

            if (!ukprn.HasValue)
            {
                context.Result = new NotFoundResult();
            }
            else
            {
                // Stash the UKPRN so additional filters can use it
                context.HttpContext.Features.Set(new ApprenticeshipProviderFeature(ukprn.Value));

                // If the action has a ProviderInfo parameter, ensure it's bound and matches this UKPRN
                var parameterInfoActionParameters = context.ActionDescriptor.Parameters
                    .Where(p => p.ParameterType == typeof(ProviderInfo))
                    .ToList();

                foreach (var p in parameterInfoActionParameters)
                {
                    var boundValue = context.ActionArguments.ContainsKey(p.Name) ? (ProviderInfo)context.ActionArguments[p.Name] : null;
                    if (boundValue == null)
                    {
                        var providerInfoCache = context.HttpContext.RequestServices.GetRequiredService<IProviderInfoCache>();
                        var providerInfo = await providerInfoCache.GetProviderInfo(ukprn.Value);
                        context.ActionArguments[p.Name] = providerInfo;
                    }
                    else if (boundValue.Ukprn != ukprn.Value)
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

    public class ApprenticeshipProviderFeature
    {
        public ApprenticeshipProviderFeature(int ukprn)
        {
            Ukprn = ukprn;
        }

        public int Ukprn { get; }
    }
}
