using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfc.CourseDirectory.WebV2.Filters
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

            // TODO: Store the status itself (not the description) i.e. A, V, PD1, PD2
            // see https://skillsfundingagency.atlassian.net/wiki/spaces/DFC/pages/873136299/CD+Beta+-+UKRLP
            var providerIsDeactivated = !(providerStatus.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                providerStatus.Equals("Verified", StringComparison.OrdinalIgnoreCase));

            if (providerIsDeactivated)
            {
                context.Result = new ViewResult()
                {
                    StatusCode = 400,
                    ViewName = "ProviderDeactivatedError"
                };
            }
        }
    }
}
