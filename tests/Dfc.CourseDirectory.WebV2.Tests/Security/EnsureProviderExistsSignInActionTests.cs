using System;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
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
            var providerId = await TestData.CreateProvider();

            var provider = await CosmosDbQueryDispatcher.Object.ExecuteQuery(new GetProviderById()
            {
                ProviderId = providerId
            });

            var signInContext = new SignInContext(new System.Security.Claims.ClaimsPrincipal())
            {
                Provider = provider,
                ProviderUkprn = provider.Ukprn,
                UserInfo = new AuthenticatedUserInfo()
                {
                    CurrentProviderId = providerId,
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
                var connection = dispatcher.Transaction.Connection;
                return connection.QuerySingleAsync<int>(
                    "select count(*) from Pttcd.Providers where ProviderId = @ProviderId",
                    new { ProviderId = providerId },
                    dispatcher.Transaction);
            });
            Assert.Equal(1, rows);
        }
    }
}
