using System;
using Dfc.CourseDirectory.WebV2.Models;

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

        public void AsDeveloper(string email, Guid userId)
        {
            IsAuthenticated = true;
            Email = email;
            UserId = userId;
            Role = "Developer";
            UKPRN = null;
            ProviderType = null;
        }

        public void AsHelpdesk(string email, Guid userId)
        {
            IsAuthenticated = true;
            Email = email;
            UserId = userId;
            Role = "Helpdesk";
            UKPRN = null;
            ProviderType = null;
        }

        public void AsProviderUser(string email, Guid userId, int ukprn, ProviderType providerType)
        {
            IsAuthenticated = true;
            Email = email;
            UserId = userId;
            Role = "Provider User";
            UKPRN = ukprn;
            ProviderType = providerType;
        }

        public void AsProviderSuperUser(string email, Guid userId, int ukprn, ProviderType providerType)
        {
            IsAuthenticated = true;
            Email = email;
            UserId = userId;
            Role = "Provider Superuser";
            UKPRN = ukprn;
            ProviderType = providerType;
        }

        public void Reset() => AsDeveloper(DefaultEmail, DefaultUserId);

        public void SetNotAuthenticated() => IsAuthenticated = false;
    }
}
