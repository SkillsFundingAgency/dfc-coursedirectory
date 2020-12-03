using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Dfc.CourseDirectory.WebV2.ViewHelpers
{
    public class RouteValuesHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RouteValuesHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IDictionary<string, string> FromQueryString()
        {
            var context = _httpContextAccessor.HttpContext;

            return context.Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
        }
    }
}
