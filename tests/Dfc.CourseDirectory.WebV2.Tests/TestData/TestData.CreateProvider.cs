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
            int ukprn,
            ProviderType providerType = ProviderType.Both,
            string providerName = "Test Provider",
            string alias = "",
            string courseDirectoryName = "",
            string marketingInformation = "",
            ApprenticeshipQAStatus apprenticeshipQAStatus = ApprenticeshipQAStatus.NotStarted)
        {
            var providerId = Guid.NewGuid();

            var result = await _cosmosDbQueryDispatcher.ExecuteQuery(new CreateProvider()
            {
                ProviderId = providerId,
                Ukprn = ukprn,
                ProviderType = providerType,
                ProviderName = providerName,
                Alias = alias,
                CourseDirectoryName = courseDirectoryName,
                MarketingInformation = marketingInformation
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
