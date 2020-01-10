using System;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.Security;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class AuthenticatedUserInfo
    {
        public const string DefaultEmail = "test.user@place.com";

        public static readonly Guid DefaultUserId = new Guid("9b8adb2a-5a26-44b9-b6a0-52846f7a4555");  // Dummy ID

        public string Email { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public Guid UserId { get; private set; }  // GUID to mirror DfE Sign In
        public string Role { get; private set; }

        public int? UKPRN { get; private set; }
        public ProviderType? ProviderType { get; private set; }

        public void AsDeveloper() => AsDeveloper(DefaultEmail, DefaultUserId);

        public void AsDeveloper(string email, Guid userId)
        {
            IsAuthenticated = true;
            Email = email;
            UserId = userId;
            Role = RoleNames.Developer;
            UKPRN = null;
            ProviderType = null;
        }

        public void AsHelpdesk() => AsHelpdesk(DefaultEmail, DefaultUserId);

        public void AsHelpdesk(string email, Guid userId)
        {
            IsAuthenticated = true;
            Email = email;
            UserId = userId;
            Role = RoleNames.Helpdesk;
            UKPRN = null;
            ProviderType = null;
        }

        public void AsProviderUser(int ukprn, ProviderType providerType) =>
            AsProviderUser(DefaultEmail, DefaultUserId, ukprn, providerType);

        public void AsProviderUser(string email, Guid userId, int ukprn, ProviderType providerType)
        {
            IsAuthenticated = true;
            Email = email;
            UserId = userId;
            Role = RoleNames.ProviderUser;
            UKPRN = ukprn;
            ProviderType = providerType;
        }

        public void AsProviderSuperUser(int ukprn, ProviderType providerType) =>
            AsProviderSuperUser(DefaultEmail, DefaultUserId, ukprn, providerType);

        public void AsProviderSuperUser(string email, Guid userId, int ukprn, ProviderType providerType)
        {
            IsAuthenticated = true;
            Email = email;
            UserId = userId;
            Role = RoleNames.ProviderSuperUser;
            UKPRN = ukprn;
            ProviderType = providerType;
        }

        public void Reset() => AsDeveloper();

        public void SetNotAuthenticated()
        {
            IsAuthenticated = false;
            Email = default;
            UserId = default;
            Role = default;
            UKPRN = default;
            ProviderType = default;
        }
    }
}
