using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Query = Dfc.CourseDirectory.Core.DataStore.Sql.Queries.CreateApprenticeshipQASubmission;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public Task<int> CreateApprenticeshipQASubmission(
            Guid providerId,
            DateTime submittedOn,
            string submittedByUserId,
            string providerMarketingInformation,
            IEnumerable<Guid> apprenticeshipIds)
        {
            return WithSqlQueryDispatcher(async dispatcher =>
            {
                var providerApprenticeships = (await dispatcher.ExecuteQuery(new GetApprenticeshipsForProvider() { ProviderId = providerId }))
                    .ToDictionary(a => a.ApprenticeshipId, a => a);

                var providerVenues = await dispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = providerId });

                var queryApps = apprenticeshipIds
                    .Select(id => providerApprenticeships[id])
                    .Select(a => new CreateApprenticeshipQASubmissionApprenticeship()
                    {
                        ApprenticeshipId = a.ApprenticeshipId,
                        ApprenticeshipMarketingInformation = a.MarketingInformation,
                        ApprenticeshipTitle = a.Standard.StandardName,
                        Locations = a.ApprenticeshipLocations.Select(l => l.ApprenticeshipLocationType switch
                        {
                            ApprenticeshipLocationType.ClassroomBased => new CreateApprenticeshipQASubmissionApprenticeshipLocation(
                                new CreateApprenticeshipQASubmissionApprenticeshipClassroomLocation()
                                {
                                    DeliveryModes = l.DeliveryModes,
                                    Radius = l.Radius.Value,
                                    VenueName = providerVenues.Single(v => v.VenueId == l.Venue.VenueId).VenueName
                                }),
                            ApprenticeshipLocationType.EmployerBased => new CreateApprenticeshipQASubmissionApprenticeshipLocation(
                                l.National == true ?
                                    new CreateApprenticeshipQASubmissionApprenticeshipEmployerLocation(new National()) :
                                    new CreateApprenticeshipQASubmissionApprenticeshipEmployerLocation(
                                        new CreateApprenticeshipQASubmissionApprenticeshipEmployerLocationRegions()
                                        {
                                            SubRegionIds = l.SubRegionIds.ToArray()
                                        })),
                            _ => throw new NotSupportedException()
                        }).ToList()
                    });

                return await dispatcher.ExecuteQuery(new Query()
                {
                    Apprenticeships = queryApps,
                    ProviderMarketingInformation = providerMarketingInformation,
                    ProviderId = providerId,
                    SubmittedByUserId = submittedByUserId,
                    SubmittedOn = submittedOn
                });
            });
        }
    }
}
