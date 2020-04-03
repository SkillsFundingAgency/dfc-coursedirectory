using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.Helpers;
using Dfc.CourseDirectory.WebV2.Helpers.Interfaces;
using Dfc.CourseDirectory.WebV2.Security;
using Dfc.CourseDirectory.WebV2.Services.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UkrlpService;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.Security
{
    public class SyncUserProviderSignInActionTests : TestBase
    {
        private SyncUserProviderSignInAction signInActionToTest;
        private IUkrlpSyncHelper _ukrlpSyncHelper;

        public SyncUserProviderSignInActionTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task WithSignInContext_EnsureProviderDataIsSynched()
        {
            // Arrange

            int providerUkprn = 01234566;

            var mockUkrlpWcfService = base.Factory.UkrlpWcfService;
            mockUkrlpWcfService.Setup(w => w.GetProviderData(providerUkprn)).Returns(this.GetProviderData(providerUkprn));

            var mockCosmosDbQueryDispatcher = base.Factory.CosmosDbQueryDispatcher;
            mockCosmosDbQueryDispatcher.Setup(w => w.ExecuteQuery(new GetProviderByUkprn() { Ukprn = providerUkprn })).Returns(this.GetExistingProvider(providerUkprn));

            _ukrlpSyncHelper = new UkrlpSyncHelper(mockUkrlpWcfService.Object, mockCosmosDbQueryDispatcher.Object, base.Factory.Clock);
            signInActionToTest = new SyncUserProviderSignInAction(_ukrlpSyncHelper);

            // Act
            await signInActionToTest.OnUserSignedIn(GetSignInContext());

            // Assert
            mockCosmosDbQueryDispatcher.Verify(v => v.ExecuteQuery<UpsertProviderUkrlpData>(null), Moq.Times.Once());
        }

        private SignInContext GetSignInContext()
        {
            return new SignInContext(this.Factory.User.ToPrincipal());
        }

        private ProviderRecordStructure GetProviderData(int ukprn)
        {
            return new ProviderRecordStructure()
            {
                // TODO : Populate data
            };
        }

        private async Task<Provider> GetExistingProvider(int ukprn)
        {
            var provider = new Provider();
            provider.UnitedKingdomProviderReferenceNumber = ukprn.ToString();
            return await Task.FromResult<Provider>(provider);
        }

        private UpsertProviderUkrlpData GetUpsertProviderData()
        {
            return new UpsertProviderUkrlpData()
            {

            };
        }
    }
}
