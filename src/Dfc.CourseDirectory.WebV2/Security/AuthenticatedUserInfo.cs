﻿using System;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2.Security
{
    public class AuthenticatedUserInfo : UserInfo
    {
        public string Role { get; set; }

        public bool IsDeveloper => Role == RoleNames.Developer;
        public bool IsHelpdesk => Role == RoleNames.Helpdesk;
        public bool IsProvider => Role == RoleNames.ProviderSuperUser || Role == RoleNames.ProviderUser;
    }
}
