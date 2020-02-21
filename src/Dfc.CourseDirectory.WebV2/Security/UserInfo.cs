namespace Dfc.CourseDirectory.WebV2.Security
{
    public class UserInfo
    {
        public string Email { get; set; }
        public string Role { get; set; }

        public bool IsDeveloper => Role == RoleNames.Developer;
        public bool IsHelpdesk => Role == RoleNames.Helpdesk;
        public bool IsProvider => Role == RoleNames.ProviderSuperUser || Role == RoleNames.ProviderUser;
    }
}
