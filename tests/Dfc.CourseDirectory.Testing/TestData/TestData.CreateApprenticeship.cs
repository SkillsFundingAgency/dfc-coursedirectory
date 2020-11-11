using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<Guid> CreateApprenticeship(
            Guid providerId,
            StandardOrFramework standardOrFramework,
            UserInfo createdBy,
            string marketingInformation = "Marketing info",
            string website = "http://provider.com/apprenticeship",
            string contactTelephone = "01234 567890",
            string contactEmail = "admin@provider.com",
            string contactWebsite = "http://provider.com",
            DateTime? createdUtc = null,
            IEnumerable<CreateApprenticeshipLocation> locations =null)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderById()
            {
                ProviderId = providerId
            });

            if (provider == null)
            {
                throw new ArgumentException("Provider does not exist.", nameof(providerId));
            }

            var apprenticeshipId = Guid.NewGuid();

            await _cosmosDbQueryDispatcher.ExecuteQuery(new CreateApprenticeship()
            {
                Id = apprenticeshipId,
                ProviderId = providerId,
                ProviderUkprn = provider.Ukprn,
                ApprenticeshipTitle = standardOrFramework.StandardOrFrameworkTitle,
                ApprenticeshipType = standardOrFramework.IsStandard ?
                    ApprenticeshipType.StandardCode :
                    ApprenticeshipType.FrameworkCode,
                StandardOrFramework = standardOrFramework,
                MarketingInformation = marketingInformation,
                Url = website,
                ContactTelephone = contactTelephone,
                ContactEmail = contactEmail,
                ContactWebsite = contactWebsite,
                ApprenticeshipLocations = locations ?? new List<CreateApprenticeshipLocation> { CreateApprenticeshipLocation.CreateNational() },
                CreatedDate = createdUtc ?? _clock.UtcNow,
                CreatedByUser = createdBy
            });

            var apprenticeship = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetApprenticeshipsByIds() { ApprenticeshipIds = new[] { apprenticeshipId } });
            await _sqlDataSync.SyncApprenticeship(apprenticeship[apprenticeshipId]);

            return apprenticeshipId;
        }
    }
}
