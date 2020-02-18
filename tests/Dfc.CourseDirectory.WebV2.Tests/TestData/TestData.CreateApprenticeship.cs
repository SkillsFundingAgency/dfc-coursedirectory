using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.Queries;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public partial class TestData
    {
        public async Task<Guid> CreateApprenticeship(int providerUkprn)
        {
            var apprenticeshipId = Guid.NewGuid();

            var result = await _cosmosDbQueryDispatcher.ExecuteQuery(new CreateApprenticeship()
            {
                ApprenticeshipId = apprenticeshipId,
                ProviderUkprn = providerUkprn
            });
            Assert.Equal(CreateApprenticeshipStatus.Ok, result);

            return apprenticeshipId;
        }
    }
}
