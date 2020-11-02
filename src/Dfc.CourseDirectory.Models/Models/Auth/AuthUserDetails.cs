using System;

namespace Dfc.CourseDirectory.Models.Models.Auth
{
    public class AuthUserDetails
    {
        public Guid? UserId { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string NameOfUser { get; set; }
        public Guid? RoleId { get; set; }
        public string RoleName { get; set; }
        public string UKPRN { get; set; }
        public Guid? ProviderID { get; set; }

        public AuthUserDetails(
            Guid? userId,
            string email,
            string userName,
            string nameOfUser,
            Guid? roleId,
            string roleName,
            string ukPrn,
            Guid? providerId)
        {
            UserId = userId;
            Email = email;
            UserName = userName;
            NameOfUser = nameOfUser;
            RoleId = roleId;
            RoleName = roleName;
            UKPRN = ukPrn;
            ProviderID = providerId;
        }

        public AuthUserDetails() { }
    }
}
