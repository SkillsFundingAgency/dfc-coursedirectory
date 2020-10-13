using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    public class AuthorizeCourseAttribute : ActionFilterAttribute
    {
        public AuthorizeCourseAttribute(string courseIdRouteParameterName = "courseId")
        {
            CourseIdRouteParameterName = courseIdRouteParameterName;
        }

        public string CourseIdRouteParameterName { get; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!Guid.TryParse(context.RouteData.Values[CourseIdRouteParameterName]?.ToString(), out var courseId))
            {
                throw new InvalidOperationException(
                    $"Could not extract course ID from '{CourseIdRouteParameterName}' route parameter.");
            }

            var services = context.HttpContext.RequestServices;
            var currentUserProvider = services.GetRequiredService<ICurrentUserProvider>();
            var providerContextProvider = services.GetRequiredService<IProviderContextProvider>();
            var providerInfoCache = services.GetRequiredService<IProviderInfoCache>();
            var providerOwnershipCache = services.GetRequiredService<IProviderOwnershipCache>();

            var providerId = await providerOwnershipCache.GetProviderForCourse(courseId);

            if (providerId == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Course, courseId);
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
