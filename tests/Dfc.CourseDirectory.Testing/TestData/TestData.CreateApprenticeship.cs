using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.Models;
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
            Func<IEnumerable<CreateApprenticeshipLocation>> Locations =null)
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
            var locations = Locations != null ? Locations.Invoke() : new CreateApprenticeshipLocation[]
                {
                    CreateApprenticeshipLocation.CreateNational()
                };

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
                ApprenticeshipLocations = locations,
                CreatedDate = createdUtc ?? _clock.UtcNow,
                CreatedByUser = createdBy
            });

            return apprenticeshipId;
        }
    }
}
