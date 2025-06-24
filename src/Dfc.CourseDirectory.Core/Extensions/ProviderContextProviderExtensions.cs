using System;
using Dfc.CourseDirectory.Core.Middleware;

namespace Dfc.CourseDirectory.Core.Extensions
{
    public static class ProviderContextProviderExtensions
    {
        public static Guid GetProviderId(
            this IProviderContextProvider providerContextProvider,
            bool withLegacyFallback = false) =>
            providerContextProvider.GetProviderContext(withLegacyFallback).ProviderInfo.ProviderId;
    }
}
