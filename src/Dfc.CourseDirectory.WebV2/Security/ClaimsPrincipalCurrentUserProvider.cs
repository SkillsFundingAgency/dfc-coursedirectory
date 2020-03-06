using System;
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

        public static AuthenticatedUserInfo MapUserInfoFromPrincipal(ClaimsPrincipal principal)
        {
            var providerIdClaim = principal.FindFirst("ProviderId");
            var providerId = providerIdClaim != null ? Guid.Parse(providerIdClaim.Value) : (Guid?)null;

            return new AuthenticatedUserInfo()
            {
                Email = principal.FindFirst("email").Value,
                FirstName = principal.FindFirst("given_name").Value,
                LastName = principal.FindFirst("family_name").Value,
                Role = principal.FindFirst(ClaimTypes.Role).Value,
                UserId = principal.FindFirst("sub").Value,
                ProviderId = providerId
            };
        }

        public AuthenticatedUserInfo GetCurrentUser()
        {
            var user = _httpContextAccessor.HttpContext.User;

            return MapUserInfoFromPrincipal(user);
        }
    }
}
