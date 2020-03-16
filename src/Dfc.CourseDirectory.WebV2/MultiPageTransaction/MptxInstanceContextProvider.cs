using Microsoft.AspNetCore.Http;

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

        public MptxInstanceContext<T> GetContext<T>()
            where T : IMptxState, new()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var feature = httpContext.Features.Get<MptxInstanceFeature>();

            if (feature == null)
            {
                return null;
            }

            return new MptxInstanceContext<T>(_stateProvider, feature.Instance);
        }
    }
}
