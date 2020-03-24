using Microsoft.AspNetCore.Http;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class MptxInstanceContextProvider
    {
        private readonly MptxInstanceContextFactory _contextFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MptxInstanceContextProvider(
            MptxInstanceContextFactory contextFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _contextFactory = contextFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public MptxInstanceContext GetContext()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var feature = httpContext.Features.Get<MptxInstanceFeature>();

            if (feature == null)
            {
                return null;
            }

            return _contextFactory.CreateContext(feature.Instance);
        }
    }
}
