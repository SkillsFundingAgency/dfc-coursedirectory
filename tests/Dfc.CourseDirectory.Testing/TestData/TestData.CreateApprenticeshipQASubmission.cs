using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Query = Dfc.CourseDirectory.Core.DataStore.Sql.Queries.CreateApprenticeshipQASubmission;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<int> CreateApprenticeshipQASubmission(
            Guid providerId,
            DateTime submittedOn,
            string submittedByUserId,
            string providerMarketingInformation,
            IEnumerable<Guid> apprenticeshipIds)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderById()
            {
                ProviderId = providerId
            });

            var apps = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetApprenticeshipsByIds()
            {
                Ukprn = int.Parse(provider.UnitedKingdomProviderReferenceNumber),
                ApprenticeshipIds = apprenticeshipIds
            });

            var queryApps = apprenticeshipIds
                .Select(id => apps[id])
                .Select(a => new CreateApprenticeshipQASubmissionApprenticeship()
                {
                    ApprenticeshipId = a.Id,
                    ApprenticeshipMarketingInformation = a.MarketingInformation,
                    ApprenticeshipTitle = a.ApprenticeshipTitle
                });

            return await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new Query()
            {
                Apprenticeships = queryApps,
                ProviderMarketingInformation = providerMarketingInformation,
                ProviderId = providerId,
                SubmittedByUserId = submittedByUserId,
                SubmittedOn = submittedOn
            }));
        }
    }
}
