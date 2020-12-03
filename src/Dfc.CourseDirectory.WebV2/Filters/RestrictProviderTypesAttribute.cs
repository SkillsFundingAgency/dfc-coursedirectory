using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    /// <summary>
    /// A filter that throws a <see cref="NotAuthorizedException"/> if the current provider does not have
    /// at least one of the specified permitted <see cref="ProviderType"/>s.
    /// </summary>
    public class RestrictProviderTypesAttribute :
        ActionFilterAttribute,
        IActionModelConvention,
        IControllerModelConvention
    {
        public RestrictProviderTypesAttribute(ProviderType permittedProviderTypes)
        {
            if (permittedProviderTypes == ProviderType.None)
            {
                throw new ArgumentException(
                    $"'{ProviderType.None}' cannot be specified for {nameof(permittedProviderTypes)}.",
                    nameof(permittedProviderTypes));
            }

            PermittedProviderTypes = permittedProviderTypes;

            // Run after the filters that assign provider context and check it's set
            Order = 100;
        }

        public ProviderType PermittedProviderTypes { get; }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var services = context.HttpContext.RequestServices;
            var providerContextProvider = services.GetRequiredService<IProviderContextProvider>();

            // Previous filter has already checked this is set
            var providerContext = providerContextProvider.GetProviderContext();

            if ((providerContext.ProviderInfo.ProviderType & PermittedProviderTypes) == 0)
            {
                // The active provider doesn't have any of the permitted provider types
                throw new NotAuthorizedException();
            }

            await next();
        }

        private void AddMetadataToAction(ActionModel action)
        {
            // Adding this metadata to the action means RedirectToProviderSelectionActionFilter will run
            // and check that provider context is assigned
            action.Properties[typeof(RequireProviderContextMarker)] = RequireProviderContextMarker.Instance;
        }

        void IActionModelConvention.Apply(ActionModel action)
        {
            AddMetadataToAction(action);
        }

        void IControllerModelConvention.Apply(ControllerModel controller)
        {
            foreach (var action in controller.Actions)
            {
                AddMetadataToAction(action);
            }
        }
    }
}
