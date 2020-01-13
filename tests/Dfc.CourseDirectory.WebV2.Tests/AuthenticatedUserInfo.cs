using System;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.Security;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public enum TestUserType { Developer, Helpdesk, ProviderUser, ProviderSuperUser }

    public class AuthenticatedUserInfo
    {
        public const string DefaultEmail = "test.user@place.com";
        public const string DefaultFirstName = "Test";
        public const string DefaultLastName = "User";

        public static readonly Guid DefaultUserId = new Guid("9b8adb2a-5a26-44b9-b6a0-52846f7a4555");  // Dummy ID

        public string Email { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public Guid UserId { get; private set; }  // GUID to mirror DfE Sign In
        public string Role { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }

        public int? UKPRN { get; private set; }
        public ProviderType? ProviderType { get; private set; }

        public void AsTestUser(TestUserType userType, int? ukprn = null)
        {
            switch (userType)
            {
                case TestUserType.Developer:
                    AsDeveloper();
                    break;
                case TestUserType.Helpdesk:
                    AsHelpdesk();
                    break;
                case TestUserType.ProviderSuperUser:
                    AsProviderSuperUser(ukprn.Value, Models.ProviderType.Both);
                    break;
                case TestUserType.ProviderUser:
                    AsProviderUser(ukprn.Value, Models.ProviderType.Both);
                    break;
                default:
                    throw new ArgumentException($"Unknown test user type: '{userType}'.", nameof(userType));
            }
        }

        public void AsDeveloper() => AsDeveloper(DefaultEmail, DefaultUserId, DefaultFirstName, DefaultLastName);

        public void AsDeveloper(string email, Guid userId, string firstName, string lastName)
        {
            IsAuthenticated = true;
            Email = email;
            UserId = userId;
            Role = RoleNames.Developer;
            FirstName = firstName;
            LastName = lastName;
            UKPRN = null;
            ProviderType = null;
        }

        public void AsHelpdesk() => AsHelpdesk(DefaultEmail, DefaultUserId, DefaultFirstName, DefaultLastName);

        public void AsHelpdesk(string email, Guid userId, string firstName, string lastName)
        {
            IsAuthenticated = true;
            Email = email;
            UserId = userId;
            Role = RoleNames.Helpdesk;
            FirstName = firstName;
            LastName = lastName;
            UKPRN = null;
            ProviderType = null;
        }

        public void AsProviderUser(int ukprn, ProviderType providerType) =>
            AsProviderUser(DefaultEmail, DefaultUserId, DefaultFirstName, DefaultLastName, ukprn, providerType);

        public void AsProviderUser(
            string email,
            Guid userId,
            string firstName,
            string lastName,
            int ukprn,
            ProviderType providerType)
        {
            IsAuthenticated = true;
            Email = email;
            UserId = userId;
            Role = RoleNames.ProviderUser;
            FirstName = firstName;
            LastName = lastName;
            UKPRN = ukprn;
            ProviderType = providerType;
        }

        public void AsProviderSuperUser(int ukprn, ProviderType providerType) =>
            AsProviderSuperUser(DefaultEmail, DefaultUserId, DefaultFirstName, DefaultLastName, ukprn, providerType);

        public void AsProviderSuperUser(
            string email,
            Guid userId,
            string firstName,
            string lastName,
            int ukprn,
            ProviderType providerType)
        {
            IsAuthenticated = true;
            Email = email;
            UserId = userId;
            Role = RoleNames.ProviderSuperUser;
            FirstName = firstName;
            LastName = lastName;
            UKPRN = ukprn;
            ProviderType = providerType;
        }

        public void Reset() => AsDeveloper();

        public void SetNotAuthenticated()
        {
            IsAuthenticated = false;
            Email = default;
            UserId = default;
            FirstName = default;
            LastName = default;
            Role = default;
            UKPRN = default;
            ProviderType = default;
        }
    }
}
