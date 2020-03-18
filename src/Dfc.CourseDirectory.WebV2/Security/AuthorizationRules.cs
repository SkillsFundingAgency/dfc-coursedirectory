using System;

namespace Dfc.CourseDirectory.WebV2.Security
{
    public static class AuthorizationRules
    {
        public static bool UserCanSubmitQASubmission(AuthenticatedUserInfo userInfo, Guid providerId) =>
            userInfo.IsDeveloper ||
            (userInfo.IsProvider && userInfo.ProviderId.Value == providerId);
    }
}
