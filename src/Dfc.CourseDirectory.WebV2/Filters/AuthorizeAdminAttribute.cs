using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    public class AuthorizeAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var services = context.HttpContext.RequestServices;
            var currentUserProvider = services.GetRequiredService<ICurrentUserProvider>();

            if (currentUserProvider.GetCurrentUser()?.IsAdmin != true)
            {
                throw new NotAuthorizedException();
            }
        }
    }
}
