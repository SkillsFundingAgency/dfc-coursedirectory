using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public partial class TestData
    {
        public async Task<Guid> CreateApprenticeship(
            Guid providerId,
            StandardOrFramework standardOrFramework,
            string marketingInformation = "Marketing info",
            string website = "http://provider.com/apprenticeship",
            string contactTelephone = "01234 567890",
            string contactEmail = "admin@provider.com",
            string contactWebsite = "http://provider.com",
            DateTime? createdUtc = null,
            UserInfo createdBy = null)
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
                ApprenticeshipLocations = new CreateApprenticeshipLocation[]
                {
                    CreateApprenticeshipLocation.CreateNational()
                },
                CreatedDate = createdUtc ?? _clock.UtcNow,
                CreatedByUser = createdBy ?? _user.ToUserInfo()
            });

            return apprenticeshipId;
        }
    }
}
