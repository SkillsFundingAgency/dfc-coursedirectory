using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class MptxInstanceContextProvider
    {
        private readonly IMptxStateProvider _stateProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MptxInstanceContextProvider(
            IMptxStateProvider stateProvider,
            IHttpContextAccessor httpContextAccessor)
        {
            _stateProvider = stateProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public MptxInstanceContext GetContext(Type stateType)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var feature = httpContext.Features.Get<MptxInstanceFeature>();

            if (feature == null)
            {
                return null;
            }

            var contextType = typeof(MptxInstanceContext<>).MakeGenericType(stateType);
            return (MptxInstanceContext)ActivatorUtilities.CreateInstance(
                provider: null,
                contextType,
                _stateProvider,
                feature.Instance);
        }
    }
}
