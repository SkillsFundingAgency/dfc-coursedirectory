using System;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Security;
using Xunit;
using EnsureProviderExistsSignInAction = Dfc.CourseDirectory.WebV2.Security.EnsureProviderExistsSignInAction;

namespace Dfc.CourseDirectory.WebV2.Tests.Security
{
    public class EnsureProviderExistsSignInActionTests : DatabaseTestBase
    {
        public EnsureProviderExistsSignInActionTests(DatabaseTestBaseFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public async Task NewProvider_InsertsProviderRecord()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var signInContext = new SignInContext(new System.Security.Claims.ClaimsPrincipal())
            {
                Provider = provider,
                ProviderUkprn = provider.Ukprn,
                UserInfo = new AuthenticatedUserInfo()
                {
                    CurrentProviderId = provider.ProviderId,
                    Email = "test.guy@provider.com",
                    FirstName = "Test",
                    LastName = "Guy",
                    Role = RoleNames.ProviderSuperUser,
                    UserId = Guid.NewGuid().ToString()
                }
            };

            // Act
            await WithSqlQueryDispatcher(async dispatcher =>
            {
                var action = new EnsureProviderExistsSignInAction(dispatcher);

                await action.OnUserSignedIn(signInContext);
            });

            // Assert
            var rows = await WithSqlQueryDispatcher(dispatcher =>
            {
                return dispatcher.Transaction.Connection.QuerySingleAsync<int>(
                    "select count(*) from Pttcd.Providers where ProviderId = @ProviderId",
                    new { ProviderId = provider.ProviderId },
                    dispatcher.Transaction);
            });
            Assert.Equal(1, rows);
        }
    }
}
