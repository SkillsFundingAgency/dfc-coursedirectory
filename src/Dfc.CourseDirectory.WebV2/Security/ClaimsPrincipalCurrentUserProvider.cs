using System;
using System.Collections.Generic;
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

        public static ClaimsPrincipal GetPrincipalFromSignInContext(SignInContext signInContext)
        {
            var claims = new List<Claim>()
            {
                new Claim("Email", signInContext.UserInfo.Email),
                new Claim("given_name", signInContext.UserInfo.FirstName),
                new Claim("family_name", signInContext.UserInfo.LastName),
                new Claim(ClaimTypes.Role, signInContext.UserInfo.Role),
                new Claim("sub", signInContext.UserInfo.UserId)
            };

            claims.Add(new Claim("OrganisationId", signInContext.DfeSignInOrganisationId));

            if (signInContext.Provider != null)
            {
                claims.Add(new Claim("ProviderId", signInContext.Provider.ProviderId.ToString()));
                claims.Add(new Claim("ProviderType", ((int)signInContext.Provider.ProviderType).ToString()));
                claims.Add(new Claim("provider_status", signInContext.Provider.ProviderStatus));
                claims.Add(new Claim("UKPRN", signInContext.Provider.Ukprn.ToString()));
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Dfe Sign In"));
        }

        public static AuthenticatedUserInfo MapUserInfoFromPrincipal(ClaimsPrincipal principal)
        {
            var providerIdClaim = principal.FindFirst("ProviderId");
            var providerId = providerIdClaim != null ? Guid.Parse(providerIdClaim.Value) : (Guid?)null;

            var ukprnClaim = principal.FindFirst("UKPRN");
            var ukprn = ukprnClaim != null ? int.Parse(ukprnClaim.Value) : (int?)null;

            return new AuthenticatedUserInfo()
            {
                Email = principal.FindFirst("email").Value,
                FirstName = principal.FindFirst("given_name").Value,
                LastName = principal.FindFirst("family_name").Value,
                Role = principal.FindFirst(ClaimTypes.Role)?.Value,
                UserId = principal.FindFirst("sub").Value, // sub == subject of claim in JWT, i.e. userId. https://tools.ietf.org/html/rfc7519#section-4.1.2
                CurrentProviderId = providerId,
                CurrentProviderUkprn = ukprn
            };
        }

        public AuthenticatedUserInfo GetCurrentUser()
        {
            var user = _httpContextAccessor.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                return null;
            }

            return MapUserInfoFromPrincipal(user);
        }
    }
}
