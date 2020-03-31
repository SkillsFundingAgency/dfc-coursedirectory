using System;
using System.Security.Claims;
using Dfc.CourseDirectory.WebV2.HttpContextFeatures;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AuthorizeApprenticeshipAttribute : ActionFilterAttribute
    {
        public AuthorizeApprenticeshipAttribute()
        {
            // Must run after VerifyApprenticeshipExistsAttribute since it sets data we need
            Order = 1;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var appProviderFeature = context.HttpContext.Features.Get<ApprenticeshipProviderFeature>();

            if (appProviderFeature == null)
            {
                throw new InvalidOperationException(
                    "Cannot resolve provider for apprenticeship. " +
                    "Ensure the action has a parameter decorated with the ApprenticeshipIdAttribute.");
            }

            var providerId = appProviderFeature.ProviderId;

            var role = context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

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
                    var userProviderId = Guid.Parse(context.HttpContext.User.FindFirst("ProviderId").Value);
                    isAuthorized = userProviderId == providerId;
                    break;
                default:
                    isAuthorized = false;
                    break;
            }

            if (!isAuthorized)
            {
                context.Result = new StatusCodeResult(403);
            }
        }
    }
}
