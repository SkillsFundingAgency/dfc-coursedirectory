using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Auth
{
    public class AuthUserDetails : ValueObject<AuthUserDetails>, IAuthUserDetails
    {
        public Guid? UserId { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string NameOfUser { get; set; }
        public Guid? RoleId { get; set; }
        public string RoleName { get; set; }
        public string UKPRN { get; set; }
        public string ProviderType { get; set; }
        public Guid? ProviderID { get; set; }

        public AuthUserDetails(
            Guid? userId,
            string email,
            string userName,
            string nameOfUser,
            Guid? roleId,
            string roleName,
            string ukPrn,
            string providerType,
            Guid? providerId)
        {
            UserId = userId;
            Email = email;
            UserName = userName;
            NameOfUser = nameOfUser;
            RoleId = roleId;
            RoleName = roleName;
            UKPRN = ukPrn;
            ProviderType = providerType;
            ProviderID = providerId;
        }

        public AuthUserDetails() { }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return UserId;
            yield return Email;
            yield return UserName;
            yield return NameOfUser;
            yield return RoleId;
            yield return RoleName;
            yield return UKPRN;
            yield return ProviderType;
            yield return ProviderID;
        }
    }
}
