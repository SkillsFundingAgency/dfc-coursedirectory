using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    public class AuthorizeTLevelAttribute : ActionFilterAttribute
    {
        public AuthorizeTLevelAttribute(string courseIdRouteParameterName = "tLevelId")
        {
            TLevelIdRouteParameterName = courseIdRouteParameterName;
        }

        public string TLevelIdRouteParameterName { get; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!Guid.TryParse(context.RouteData.Values[TLevelIdRouteParameterName]?.ToString(), out var tLevelId))
            {
                throw new InvalidOperationException(
                    $"Could not extract T Level ID from '{TLevelIdRouteParameterName}' route parameter.");
            }

            var services = context.HttpContext.RequestServices;
            var currentUserProvider = services.GetRequiredService<ICurrentUserProvider>();
            var providerContextProvider = services.GetRequiredService<IProviderContextProvider>();
            var providerInfoCache = services.GetRequiredService<IProviderInfoCache>();
            var providerOwnershipCache = services.GetRequiredService<IProviderOwnershipCache>();

            var providerId = await providerOwnershipCache.GetProviderForTLevel(tLevelId);

            if (providerId == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.TLevel, tLevelId);
            }

            if (!IsAuthorized())
            {
                throw new NotAuthorizedException();
            }

            var providerInfo = await providerInfoCache.GetProviderInfo(providerId.Value);
            providerContextProvider.SetProviderContext(new ProviderContext(providerInfo));

            await next();

            bool IsAuthorized()
            {
                var currentUser = currentUserProvider.GetCurrentUser();

                if (currentUser == null)
                {
                    return false;
                }

                if (currentUser.Role == RoleNames.Developer || currentUser.Role == RoleNames.Helpdesk)
                {
                    return true;
                }

                if ((currentUser.Role == RoleNames.ProviderUser || currentUser.Role == RoleNames.ProviderSuperUser) &&
                    currentUser.CurrentProviderId == providerId)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
