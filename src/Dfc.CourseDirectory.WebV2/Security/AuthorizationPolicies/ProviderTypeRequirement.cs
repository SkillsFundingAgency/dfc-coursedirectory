using Dfc.CourseDirectory.Core.Models;
using Microsoft.AspNetCore.Authorization;

namespace Dfc.CourseDirectory.WebV2.Security.AuthorizationPolicies
{
    /// <summary>
    /// Implements an <see cref="IAuthorizationRequirement"/> which specifies the <c>ProviderType</c>(s)
    /// that the current provider must have.
    /// </summary>
    public class ProviderTypeRequirement : IAuthorizationRequirement
    {
        public ProviderTypeRequirement(ProviderType providerType)
        {
            ProviderType = providerType;
        }

        public ProviderType ProviderType { get; }
    }
}
