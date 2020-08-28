using System.Collections.Generic;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Dfc.CourseDirectory.WebV2
{
    public static class RedirectToActionResultExtensions
    {
        public static RedirectToActionResult WithProviderContext(
            this RedirectToActionResult result,
            ProviderContext providerContext)
        {
            var routeValues = (IDictionary<string, object>)result.RouteValues ?? new Dictionary<string, object>();
            routeValues[ProviderContextResourceFilter.RouteValueKey] = providerContext.ProviderInfo.ProviderId;

            result.RouteValues = new RouteValueDictionary(routeValues);

            return result;
        }
    }
}
