using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Dfc.CourseDirectory.WebV2.Security.AuthorizationPolicies
{
    public class ProviderTypeAuthorizationHandler : AuthorizationHandler<ProviderTypeRequirement>
    {
        private readonly IProviderContextProvider _providerContextProvider;

        public ProviderTypeAuthorizationHandler(IProviderContextProvider providerContextProvider)
        {
            _providerContextProvider = providerContextProvider;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ProviderTypeRequirement requirement)
        {
            var providerContext = _providerContextProvider.GetProviderContext();
            
            if (providerContext == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var providerType = providerContext.ProviderInfo.ProviderType;

            if ((providerType & requirement.ProviderType) != 0)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
