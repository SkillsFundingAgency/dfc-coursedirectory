using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ApprenticeshipIdAttribute : ActionFilterAttribute
    {
        private const string DefaultParameterName = "apprenticeshipId";

        public ApprenticeshipIdAttribute(string parameterName)
        {
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            Order = 0;
        }

        public ApprenticeshipIdAttribute()
            : this(DefaultParameterName)
        {
        }

        public string ParameterName { get; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.TryGetValue(ParameterName, out var apprenticeshipId))
            {
                throw new InvalidOperationException($"Action does not contain a parameter named '{ParameterName}'.");
            }

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
                    else if (boundValue.UKPRN != ukprn.Value)
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
            UKPRN = ukprn;
        }

        public int UKPRN { get; }
    }
}
