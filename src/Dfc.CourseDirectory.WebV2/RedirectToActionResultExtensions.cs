using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Dfc.CourseDirectory.WebV2
{
    public static class RedirectToActionResultExtensions
    {
        public static RedirectToActionResult WithCurrentProvider(
            this RedirectToActionResult result,
            ProviderInfo providerInfo)
        {
            var routeValues = (IDictionary<string, object>)result.RouteValues ?? new Dictionary<string, object>();
            routeValues[CurrentProviderModelBinder.QueryParameterName] = providerInfo.Ukprn;

            result.RouteValues = new RouteValueDictionary(routeValues);

            return result;
        }
    }
}
