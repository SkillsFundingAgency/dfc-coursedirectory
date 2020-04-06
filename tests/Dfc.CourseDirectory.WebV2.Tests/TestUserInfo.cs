using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public enum TestUserType { Developer, Helpdesk, ProviderUser, ProviderSuperUser }

    public class TestUserInfo
    {
        public const string DefaultEmail = "test.user@place.com";
        public const string DefaultFirstName = "Test";
        public const string DefaultLastName = "User";
        public const string DefaultProviderStatus = "Active";

        public static readonly string DefaultUserId = new Guid("9b8adb2a-5a26-44b9-b6a0-52846f7a4555").ToString();

        private readonly IServiceScopeFactory _serviceScopeFactory;

        public TestUserInfo(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public string Email { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public string UserId { get; private set; }
        public string Role { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }

        public Guid? ProviderId { get; private set; }
        public ProviderType? ProviderType { get; private set; }
        public string ProviderStatus { get; set; }

        public Task AsTestUser(TestUserType userType, Guid? providerId = null)
        {
            if ((userType == TestUserType.ProviderSuperUser || userType == TestUserType.ProviderUser) &&
                !providerId.HasValue)
            {
                throw new ArgumentNullException(nameof(providerId));
            }

            switch (userType)
            {
                case TestUserType.Developer:
                    return AsDeveloper();
                case TestUserType.Helpdesk:
                    return AsHelpdesk();
                case TestUserType.ProviderSuperUser:
                    return AsProviderSuperUser(providerId.Value, Models.ProviderType.Both);
                case TestUserType.ProviderUser:
                    return AsProviderUser(providerId.Value, Models.ProviderType.Both);
                default:
                    throw new ArgumentException($"Unknown test user type: '{userType}'.", nameof(userType));
            }
        }

        public Task AsDeveloper() => AsDeveloper(DefaultEmail, DefaultUserId, DefaultFirstName, DefaultLastName);

        public Task AsDeveloper(string email, string userId, string firstName, string lastName)
        {
            IsAuthenticated = true;
            Email = email;
            UserId = userId;
            Role = RoleNames.Developer;
            FirstName = firstName;
            LastName = lastName;
            ProviderId = null;
            ProviderType = null;

            return RecordSignIn();
        }

        public Task AsHelpdesk() => AsHelpdesk(DefaultEmail, DefaultUserId, DefaultFirstName, DefaultLastName);

        public Task AsHelpdesk(string email, string userId, string firstName, string lastName)
        {
            IsAuthenticated = true;
            Email = email;
            UserId = userId;
            Role = RoleNames.Helpdesk;
            FirstName = firstName;
            LastName = lastName;
            ProviderId = ProviderId;
            ProviderType = null;
            ProviderStatus = null;

            return RecordSignIn();
        }

        public Task AsProviderUser(Guid providerId, ProviderType providerType) =>
            AsProviderUser(providerId, providerType, DefaultProviderStatus);

        public Task AsProviderUser(Guid providerId, ProviderType providerType, string providerStatus) =>
            AsProviderUser(DefaultEmail, DefaultUserId, DefaultFirstName, DefaultLastName, providerId, providerType, providerStatus);

        public Task AsProviderUser(
            string email,
            string userId,
            string firstName,
            string lastName,
            Guid providerId,
            ProviderType providerType,
            string providerStatus)
        {
            IsAuthenticated = true;
            Email = email;
            UserId = userId;
            Role = RoleNames.ProviderUser;
            FirstName = firstName;
            LastName = lastName;
            ProviderId = providerId;
            ProviderType = providerType;
            ProviderStatus = providerStatus;

            return RecordSignIn();
        }

        public Task AsProviderSuperUser(Guid providerId, ProviderType providerType) =>
            AsProviderSuperUser(providerId, providerType, DefaultProviderStatus);

        public Task AsProviderSuperUser(Guid providerId, ProviderType providerType, string providerStatus) =>
            AsProviderSuperUser(DefaultEmail, DefaultUserId, DefaultFirstName, DefaultLastName, providerId, providerType, providerStatus);

        public Task AsProviderSuperUser(
            string email,
            string userId,
            string firstName,
            string lastName,
            Guid providerId,
            ProviderType providerType,
            string providerStatus)
        {
            IsAuthenticated = true;
            Email = email;
            UserId = userId;
            Role = RoleNames.ProviderSuperUser;
            FirstName = firstName;
            LastName = lastName;
            ProviderId = providerId;
            ProviderType = providerType;
            ProviderStatus = providerStatus;

            return RecordSignIn();
        }

        public Task Reset() => AsDeveloper();

        public void SetNotAuthenticated()
        {
            IsAuthenticated = false;
            Email = default;
            UserId = default;
            FirstName = default;
            LastName = default;
            Role = default;
            ProviderId = default;
            ProviderType = default;
            ProviderStatus = null;
        }

        public ClaimsPrincipal ToPrincipal()
        {
            var claims = new List<Claim>()
            {
                new Claim("user_id", UserId.ToString()),
                new Claim("sub", UserId.ToString()),
                new Claim("email", Email),
                new Claim("given_name", FirstName),
                new Claim("family_name", LastName),
                new Claim(ClaimTypes.Role, Role)
            };

            if (ProviderId.HasValue)
            {
                claims.AddRange(new List<Claim>()
                {
                    new Claim("ProviderId", ProviderId.Value.ToString()),
                    new Claim("ProviderType", ProviderType.Value.ToString()),
                    new Claim("provider_status", ProviderStatus)
                    // These claims are populated in the real app but are not required here (yet):
                    // organisation - JSON from DfE Sign In API call
                    // OrganisationId - GUID Org ID for DfE API call
                });
            }

            var identity = new ClaimsIdentity(claims, "Test");
            return new ClaimsPrincipal(identity);
        }

        public UserInfo ToUserInfo() => new UserInfo()
        {
            UserId = UserId,
            Email = Email,
            FirstName = FirstName,
            LastName = LastName
        };

        private async Task RecordSignIn()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var signInTracker = scope.ServiceProvider.GetRequiredService<SignInTracker>();

                var principal = ToPrincipal();
                await signInTracker.RecordSignIn(
                    new AuthenticatedUserInfo()
                    {
                        Email = Email,
                        FirstName = FirstName,
                        LastName = LastName,
                        CurrentProviderId = ProviderId,
                        Role = Role,
                        UserId = UserId
                    });

                // REVIEW: Is there a better way of doing this?
                var transaction = scope.ServiceProvider.GetRequiredService<SqlTransaction>();
                transaction.Commit();
            }
        }
    }
}
