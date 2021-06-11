using System;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.WebV2.Security
{
    public class AuthenticatedUserInfo : UserInfo
    {
        public string Role { get; set; }
        public Guid? CurrentProviderId { get; set; }
        public int? CurrentProviderUkprn { get; set; }

        public bool IsAdmin => IsDeveloper || IsHelpdesk;
        public bool IsDeveloper => Role == RoleNames.Developer;
        public bool IsHelpdesk => Role == RoleNames.Helpdesk;
        public bool IsProvider => Role == RoleNames.ProviderSuperUser || Role == RoleNames.ProviderUser;
    }
}
