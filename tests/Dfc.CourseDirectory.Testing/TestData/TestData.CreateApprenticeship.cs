using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public Task<Apprenticeship> CreateApprenticeship(
            Guid providerId,
            Standard standard,
            UserInfo createdBy,
            ApprenticeshipStatus status = ApprenticeshipStatus.Live,
            string marketingInformation = "Marketing info",
            string website = "http://provider.com/apprenticeship",
            string contactTelephone = "01234 567890",
            string contactEmail = "admin@provider.com",
            string contactWebsite = "http://provider.com",
            DateTime? createdOn = null,
            IEnumerable<CreateApprenticeshipLocation> locations = null)
        {
            locations ??= new[]
            {
                CreateApprenticeshipLocation.CreateNationalEmployerBased()
            };

            return WithSqlQueryDispatcher(async dispatcher =>
            {
                var apprenticeshipId = Guid.NewGuid();

                await dispatcher.ExecuteQuery(new CreateApprenticeship()
                {
                    ApprenticeshipId = apprenticeshipId,
                    ProviderId = providerId,
                    Status = status,
                    Standard = standard,
                    MarketingInformation = marketingInformation,
                    ApprenticeshipWebsite = website,
                    ContactEmail = contactEmail,
                    ContactTelephone = contactTelephone,
                    ContactWebsite = contactWebsite,
                    ApprenticeshipLocations = locations,
                    CreatedBy = createdBy,
                    CreatedOn = createdOn ?? _clock.UtcNow
                });

                return await dispatcher.ExecuteQuery(new GetApprenticeship() { ApprenticeshipId = apprenticeshipId });
            });
        }
    }
}
