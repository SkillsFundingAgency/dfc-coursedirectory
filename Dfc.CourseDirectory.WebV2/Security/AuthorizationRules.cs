﻿using System;

namespace Dfc.CourseDirectory.Core.Security
{
    public static class AuthorizationRules
    {
        public static bool CanSubmitQASubmission(AuthenticatedUserInfo userInfo, Guid providerId) =>
            userInfo.IsDeveloper ||
            (userInfo.IsProvider && userInfo.CurrentProviderId.Value == providerId);

        public static bool CanUpdateProviderDisplayName(AuthenticatedUserInfo userInfo) =>
            userInfo.IsDeveloper || userInfo.IsHelpdesk || userInfo.Role == RoleNames.ProviderSuperUser;

        public static bool CanUpdateProviderType(AuthenticatedUserInfo userInfo) =>
            userInfo.IsDeveloper || userInfo.IsHelpdesk;

        public static bool CanUpdateProviderMarketingInformation(AuthenticatedUserInfo userInfo) =>
            userInfo.IsDeveloper || userInfo.IsHelpdesk;
    }
}
