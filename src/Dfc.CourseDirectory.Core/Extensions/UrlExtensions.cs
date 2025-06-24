using Dfc.CourseDirectory.Core.Middleware;
using Flurl;

namespace Dfc.CourseDirectory.Core.Extensions
{
    public static class UrlExtensions
    {
        public static Url WithProviderContext(this Url url, ProviderContext providerContext) =>
            url.SetQueryParam(ProviderContextMiddleware.RouteValueKey, providerContext.ProviderInfo.ProviderId);
    }
}
