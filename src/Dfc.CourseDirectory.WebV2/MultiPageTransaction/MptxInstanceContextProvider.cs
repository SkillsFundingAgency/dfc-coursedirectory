using Microsoft.AspNetCore.Http;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class MptxInstanceContextProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MptxInstanceContextProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public MptxInstanceContext GetContext()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var feature = httpContext.Features.Get<MptxInstanceContextFeature>();

            if (feature == null)
            {
                return null;
            }

            return feature.InstanceContext;
        }
    }
}
