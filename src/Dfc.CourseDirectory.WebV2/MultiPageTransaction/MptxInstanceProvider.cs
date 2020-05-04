using Microsoft.AspNetCore.Http;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class MptxInstanceProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MptxInstanceProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public MptxInstance GetInstance()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var feature = httpContext.Features.Get<MptxInstanceFeature>();

            if (feature == null)
            {
                return null;
            }

            return feature.Instance;
        }
    }
}
