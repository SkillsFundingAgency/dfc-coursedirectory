using System;

namespace Dfc.CourseDirectory.WebV2.Security
{
    public static class AuthorizationRules
    {
        public static bool CanSubmitQASubmission(AuthenticatedUserInfo userInfo, Guid providerId) =>
            userInfo.IsDeveloper ||
            (userInfo.IsProvider && userInfo.CurrentProviderId.Value == providerId);

        public static bool CanUpdateProviderCourseDirectoryName(AuthenticatedUserInfo userInfo) =>
            userInfo.IsDeveloper;

        public static bool CanUpdateProviderType(AuthenticatedUserInfo userInfo) =>
            userInfo.IsDeveloper || userInfo.IsHelpdesk;

        public static bool CanUpdateProviderMarketingInformation(AuthenticatedUserInfo userInfo) =>
            userInfo.IsDeveloper;
    }
}
