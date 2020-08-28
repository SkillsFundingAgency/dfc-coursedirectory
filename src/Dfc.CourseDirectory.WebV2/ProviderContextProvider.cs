using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.HttpContextFeatures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Dfc.CourseDirectory.WebV2
{
    public class ProviderContextProvider : IProviderContextProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _env;
        private readonly IProviderInfoCache _providerInfoCache;

        public ProviderContextProvider(
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env,
            IProviderInfoCache providerInfoCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _env = env;
            _providerInfoCache = providerInfoCache;
        }

        public async Task<ProviderContext> GetProviderContext()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var feature = httpContext.Features.Get<ProviderContextFeature>();

            if (feature != null)
            {
                return feature.ProviderContext;
            }

            if (!_env.IsTesting())
            {
                // Legacy page support - old pages use UKPRN in session for holding onto provider
                // so look for that as a fallback.
                // N.B. It's disabled for tests so that new features (i.e. those that have test coverage)
                // are required to use new mechanism.

                var ukprn = httpContext.Session.GetInt32("UKPRN");
                if (ukprn.HasValue)
                {
                    var providerId = await _providerInfoCache.GetProviderIdForUkprn(ukprn.Value);
                    if (providerId == null)
                    {
                        return null;
                    }

                    var providerInfo = await _providerInfoCache.GetProviderInfo(providerId.Value);
                    if (providerInfo == null)
                    {
                        return null;
                    }

                    return new ProviderContext(providerInfo);
                }
            }

            return null;
        }

        public void SetProviderContext(ProviderContext providerContext)
        {
            if (providerContext == null)
            {
                throw new ArgumentNullException(nameof(providerContext));
            }

            var httpContext = _httpContextAccessor.HttpContext;
            httpContext.Features.Set(new ProviderContextFeature(providerContext));
        }
    }
}
