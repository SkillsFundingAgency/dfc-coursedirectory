using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    public sealed class AssignLegacyProviderContextFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var env = context.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();

            if (!env.IsTesting())
            {
                var providerContextProvider = context.HttpContext.RequestServices.GetRequiredService<IProviderContextProvider>();
                providerContextProvider.AssignLegacyProviderContext();
            }
        }
    }
}
