using System;

namespace Dfc.CourseDirectory.WebV2
{
    public static class ProviderContextProviderExtensions
    {
        public static Guid GetProviderId(this IProviderContextProvider providerContextProvider) =>
            providerContextProvider.GetProviderContext().ProviderInfo.ProviderId;
    }
}
