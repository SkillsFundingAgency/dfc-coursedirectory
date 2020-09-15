using System;
using Dfc.CourseDirectory.WebV2.HttpContextFeatures;
using Microsoft.AspNetCore.Http;

namespace Dfc.CourseDirectory.WebV2
{
    public class ProviderContextProvider : IProviderContextProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProviderContextProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void AssignLegacyProviderContext()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var feature = httpContext.Features.Get<ProviderContextFeature>();

            if (feature == null)
            {
                throw new InvalidOperationException("No provider context set.");
            }

            httpContext.Session.SetInt32("UKPRN", feature.ProviderContext.ProviderInfo.Ukprn);
        }

        public ProviderContext GetProviderContext()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var feature = httpContext.Features.Get<ProviderContextFeature>();
            return feature?.ProviderContext;
        }

        public void SetProviderContext(ProviderContext providerContext)
        {
            if (providerContext == null)
            {
                throw new ArgumentNullException(nameof(providerContext));
            }

            var httpContext = _httpContextAccessor.HttpContext;

            var currentContextFeature = httpContext.Features.Get<ProviderContextFeature>();

            if (currentContextFeature != null &&
                currentContextFeature.ProviderContext.Strict &&
                currentContextFeature.ProviderContext.ProviderInfo.ProviderId != providerContext.ProviderInfo.ProviderId)
            {
                throw new InvalidOperationException(
                    $"Provider context has already been set for another provider: '{currentContextFeature.ProviderContext.ProviderInfo.ProviderId}'.");
            }

            httpContext.Features.Set(new ProviderContextFeature(providerContext));
        }
    }
}
