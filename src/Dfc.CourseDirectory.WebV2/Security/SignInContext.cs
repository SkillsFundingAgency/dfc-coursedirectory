using System.Security.Claims;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.WebV2.Security
{
    public class SignInContext
    {
        public SignInContext(ClaimsPrincipal originalPrincipal)
        {
            OriginalPrincipal = originalPrincipal;
        }

        public AuthenticatedUserInfo UserInfo { get; set; }
        public Provider Provider { get; set; }
        public string DfeSignInOrganisationId { get; set; }
        public int? ProviderUkprn { get; set; }
        public ClaimsPrincipal OriginalPrincipal { get; }
    }
}
