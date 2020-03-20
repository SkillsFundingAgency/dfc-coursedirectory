using Dfc.CourseDirectory.WebV2.Filters;
using Flurl;

namespace Dfc.CourseDirectory.WebV2
{
    public static class UrlExtensions
    {
        public static Url WithProviderContext(this Url url, ProviderInfo providerInfo)
            => url.SetQueryParam(ProviderContextResourceFilter.RouteValueKey, providerInfo.ProviderId);
    }
}
