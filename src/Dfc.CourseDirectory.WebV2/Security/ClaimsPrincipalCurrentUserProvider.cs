using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
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
                claims.Add(new Claim("ProviderId", signInContext.Provider.Id.ToString()));
                claims.Add(new Claim("ProviderType", signInContext.Provider.ProviderType.ToString()));
                claims.Add(new Claim("provider_status", signInContext.Provider.ProviderStatus));

                // This claim is kept around to keep the old UI bits working.
                // New bits should use ProviderId instead
                claims.Add(new Claim("UKPRN", signInContext.Provider.Ukprn.ToString()));
            }
            else
            {
                // TODO: Old UI uses this to figure out what UI to show so we need to keep it around for now.
                // New UI shouldn't depend on this so we should omit this after we've migrated all the UI
                claims.Add(new Claim("ProviderType", "Both"));
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Dfe Sign In"));
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
                Role = principal.FindFirst(ClaimTypes.Role)?.Value,
                UserId = principal.FindFirst("sub").Value,
                ProviderId = providerId
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
