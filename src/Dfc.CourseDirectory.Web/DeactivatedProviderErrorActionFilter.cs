using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfc.CourseDirectory.Web
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AllowDeactivatedProviderAttribute : Attribute
    {
    }

    public class DeactivatedProviderErrorActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var providerStatus = context.HttpContext.User.FindFirst("provider_status")?.Value;

            if (string.IsNullOrEmpty(providerStatus))
            {
                return;
            }

            // If action or controller is decorated with AllowDeactivatedProviderAttribute then don't show an error
            if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                if (controllerActionDescriptor.MethodInfo.GetCustomAttribute<AllowDeactivatedProviderAttribute>() != null ||
                    controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<AllowDeactivatedProviderAttribute>() != null)
                {
                    return;
                }
            }

            var providerIsDeactivated = !(providerStatus.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                providerStatus.Equals("Verified", StringComparison.OrdinalIgnoreCase));

            if (providerIsDeactivated)
            {
                context.Result = new ViewResult() { ViewName = "ProviderDeactivatedError" };
            }
        }
    }
}
