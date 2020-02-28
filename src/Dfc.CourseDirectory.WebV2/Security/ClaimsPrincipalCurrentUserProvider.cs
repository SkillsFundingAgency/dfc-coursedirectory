using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Dfc.CourseDirectory.WebV2.Security
{
    public class ClaimsPrincipalCurrentUserProvider : ICurrentUserProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimsPrincipalCurrentUserProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public AuthenticatedUserInfo GetCurrentUser()
        {
            var user = _httpContextAccessor.HttpContext.User;

            return new AuthenticatedUserInfo()
            {
                Email = user.FindFirst("email").Value,
                Role = user.FindFirst(ClaimTypes.Role).Value
            };
        }
    }
}
