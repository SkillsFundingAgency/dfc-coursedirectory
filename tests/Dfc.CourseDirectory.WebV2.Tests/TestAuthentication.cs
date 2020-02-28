using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class TestAuthenticationOptions : AuthenticationSchemeOptions
    {
    }

    public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationOptions>
    {
        private readonly TestUserInfo _authenticatedUserInfo;

        public TestAuthenticationHandler(
            TestUserInfo authenticatedUserInfo,
            IOptionsMonitor<TestAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _authenticatedUserInfo = authenticatedUserInfo;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            AuthenticateResult result;

            if (_authenticatedUserInfo.IsAuthenticated)
            {
                var claims = new List<Claim>()
                {
                    new Claim("user_id", _authenticatedUserInfo.UserId.ToString()),
                    new Claim("sub", _authenticatedUserInfo.UserId.ToString()),
                    new Claim("email", _authenticatedUserInfo.Email),
                    new Claim("given_name", _authenticatedUserInfo.FirstName),
                    new Claim("family_name", _authenticatedUserInfo.LastName),
                    new Claim(ClaimTypes.Role, _authenticatedUserInfo.Role)
                };

                if (_authenticatedUserInfo.UKPRN.HasValue)
                {
                    claims.AddRange(new List<Claim>()
                    {
                        new Claim("UKPRN", _authenticatedUserInfo.UKPRN.Value.ToString()),
                        new Claim("ProviderType", _authenticatedUserInfo.ProviderType.Value.ToString()),
                        new Claim("provider_status", _authenticatedUserInfo.ProviderStatus)
                        // These claims are populated in the real app but are not required here (yet):
                        // organisation - JSON from DfE Sign In API call
                        // OrganisationId - GUID Org ID for DfE API call
                    });
                }

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);

                result = AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
            }
            else
            {
                result = AuthenticateResult.NoResult();
            }

            return Task.FromResult(result);
        }
    }
}
