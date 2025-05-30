using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Tests.Data
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
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
            _authenticatedUserInfo = authenticatedUserInfo;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            AuthenticateResult result;

            if (_authenticatedUserInfo.IsAuthenticated)
            {
                var principal = _authenticatedUserInfo.ToPrincipal();
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
