using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    public class AuthorizeVenueAttribute : ActionFilterAttribute
    {
        public AuthorizeVenueAttribute(string venueIdRouteParameterName = "venueId")
        {
            VenueIdRouteParameterName = venueIdRouteParameterName;
        }

        public string VenueIdRouteParameterName { get; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!Guid.TryParse(context.RouteData.Values[VenueIdRouteParameterName]?.ToString(), out var venueId))
            {
                throw new InvalidOperationException(
                    $"Could not extract venue ID from '{VenueIdRouteParameterName}' route parameter.");
            }

            var services = context.HttpContext.RequestServices;
            var currentUserProvider = services.GetRequiredService<ICurrentUserProvider>();
            var providerContextProvider = services.GetRequiredService<IProviderContextProvider>();
            var providerInfoCache = services.GetRequiredService<IProviderInfoCache>();
            var providerOwnershipCache = services.GetRequiredService<IProviderOwnershipCache>();

            var providerId = await providerOwnershipCache.GetProviderForVenue(venueId);

            if (providerId == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Venue, venueId);
            }

            if (IsAuthorized())
            {
                var providerInfo = await providerInfoCache.GetProviderInfo(providerId.Value);
                providerContextProvider.SetProviderContext(new ProviderContext(providerInfo));

                await next();
            }
            else
            {
                throw new NotAuthorizedException();
            }

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
