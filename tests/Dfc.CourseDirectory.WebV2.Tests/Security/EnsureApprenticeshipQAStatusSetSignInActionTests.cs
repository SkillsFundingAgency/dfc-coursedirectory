using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Security;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.Security
{
    public class EnsureApprenticeshipQAStatusSetSignInActionTests : DatabaseTestBase
    {
        public EnsureApprenticeshipQAStatusSetSignInActionTests(DatabaseTestBaseFixture fixture)
            : base(fixture)
        {
        }

        [Theory]
        [InlineData(ProviderType.Apprenticeships)]
        [InlineData(ProviderType.FE | ProviderType.Apprenticeships)]
        public async Task NewApprenticeshipProvider_SetsApprenticeshipQAStatus(ProviderType providerType)
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerType: providerType,
                apprenticeshipQAStatus: null);

            var cosmosProvider = await CosmosDbQueryDispatcher.Object.ExecuteQuery(new Core.DataStore.CosmosDb.Queries.GetProviderById()
            {
                ProviderId = provider.ProviderId
            });

            var signInContext = new SignInContext(new System.Security.Claims.ClaimsPrincipal())
            {
                Provider = cosmosProvider,
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
                var action = new EnsureApprenticeshipQAStatusSetSignInAction(dispatcher);

                await action.OnUserSignedIn(signInContext);
            });

            // Assert
            var qaStatus = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = provider.ProviderId
                }));
            Assert.Equal(ApprenticeshipQAStatus.NotStarted, qaStatus);
        }

        [Theory]
        [InlineData(ProviderType.FE)]
        public async Task NewFEOnlyProvider_DoesNotSetApprenticeshipQAStatus(ProviderType providerType)
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerType: providerType,
                apprenticeshipQAStatus: null);

            var cosmosProvider = await CosmosDbQueryDispatcher.Object.ExecuteQuery(new Core.DataStore.CosmosDb.Queries.GetProviderById()
            {
                ProviderId = provider.ProviderId
            });

            var signInContext = new SignInContext(new System.Security.Claims.ClaimsPrincipal())
            {
                Provider = cosmosProvider,
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
                var action = new EnsureApprenticeshipQAStatusSetSignInAction(dispatcher);

                await action.OnUserSignedIn(signInContext);
            });

            // Assert
            var qaStatus = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = provider.ProviderId
                }));
            Assert.Null(qaStatus);
        }
    }
}
