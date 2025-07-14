using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Dfc.CourseDirectory.Core.Security
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

            var schemaUrlString = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/";

            var email = principal.FindFirst("email")?.Value ??
                    principal.FindFirst($"{schemaUrlString}emailaddress")?.Value;

            var firstName = principal.FindFirst("given_name")?.Value ??
                    principal.FindFirst($"{schemaUrlString}givenname")?.Value;

            var lastName = principal.FindFirst("family_name")?.Value ??
                           principal.FindFirst($"{schemaUrlString}surname")?.Value;

            var role = principal.FindFirst(ClaimTypes.Role)?.Value;

            var userId = principal.FindFirst("sub")?.Value ??
                         principal.FindFirst($"{schemaUrlString}nameidentifier")?.Value;

            return new AuthenticatedUserInfo()
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Role = role,
                UserId = userId,
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
