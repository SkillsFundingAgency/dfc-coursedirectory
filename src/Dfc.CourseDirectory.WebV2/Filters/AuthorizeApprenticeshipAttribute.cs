using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.HttpContextFeatures;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AuthorizeApprenticeshipAttribute : ActionFilterAttribute
    {
        private readonly string routeParamName;

        public AuthorizeApprenticeshipAttribute(string routeParamName = "ApprenticeshipId")
        {
            // Must run after VerifyApprenticeshipExistsAttribute since it sets data we need
            Order = 1;
            this.routeParamName = routeParamName;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {

            if (!Guid.TryParse(context.RouteData.Values[routeParamName]?.ToString(), out var apprenticeshipId))
            {
                throw new InvalidOperationException(
                    $"Could not extract Apprenticeship ID from '{routeParamName}' route parameter.");
            }

            var role = context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            var services = context.HttpContext.RequestServices;
            var providerOwnershipCache = services.GetRequiredService<IProviderOwnershipCache>();
            var providerId = await providerOwnershipCache.GetProviderForApprenticeship(apprenticeshipId);

            if (providerId == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Apprenticeship, apprenticeshipId);
            }

            bool isAuthorized;

            // User must be Admin, Helpdesk or be a Provider {Super}User for this specific provider
            switch (role)
            {
                case RoleNames.Developer:
                case RoleNames.Helpdesk:
                    isAuthorized = true;
                    break;
                case RoleNames.ProviderSuperUser:
                case RoleNames.ProviderUser:
                    var currentUserProvider = services.GetRequiredService<ICurrentUserProvider>();
                    isAuthorized = currentUserProvider.GetCurrentUser().CurrentProviderId == providerId;
                    break;
                default:
                    isAuthorized = false;
                    break;
            }

            if (!isAuthorized)
            {
                context.Result = new StatusCodeResult(403);
            }

            var providerInfoCache = services.GetRequiredService<IProviderInfoCache>();
            var providerInfo = await providerInfoCache.GetProviderInfo(providerId.Value);
            var providerContextProvider = services.GetRequiredService<IProviderContextProvider>();
            providerContextProvider.SetProviderContext(new ProviderContext(providerInfo));

            await next();



        }
    }
}
