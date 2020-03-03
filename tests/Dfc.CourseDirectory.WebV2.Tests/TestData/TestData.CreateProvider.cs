using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.Queries;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public partial class TestData
    {
        public async Task<Guid> CreateProvider(
            int ukprn = 12345,
            string providerName = "Test Provider",
            ApprenticeshipQAStatus apprenticeshipQAStatus = ApprenticeshipQAStatus.NotStarted)
        {
            var providerId = Guid.NewGuid();

            var result = await _cosmosDbQueryDispatcher.ExecuteQuery(new CreateProvider()
            {
                ProviderId = providerId,
                Ukprn = ukprn,
                ProviderName = providerName
            });
            Assert.Equal(CreateProviderResult.Ok, result);

            if (apprenticeshipQAStatus != ApprenticeshipQAStatus.NotStarted)
            {
                await WithSqlQueryDispatcher(
                    dispatcher => dispatcher.ExecuteQuery(new SetProviderApprenticeshipQAStatus()
                    {
                        ProviderId = providerId,
                        ApprenticeshipQAStatus = apprenticeshipQAStatus
                    }));
            }

            return providerId;
        }
    }
}
