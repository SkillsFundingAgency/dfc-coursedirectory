﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
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
                Ukprn = provider.Ukprn,
                ApprenticeshipIds = apprenticeshipIds
            });

            var providerVenues = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetAllVenuesForProvider()
            {
                ProviderUkprn = provider.Ukprn
            });

            var queryApps = apprenticeshipIds
                .Select(id => apps[id])
                .Select(a => new CreateApprenticeshipQASubmissionApprenticeship()
                {
                    ApprenticeshipId = a.Id,
                    ApprenticeshipMarketingInformation = a.MarketingInformation,
                    ApprenticeshipTitle = a.ApprenticeshipTitle,
                    Locations = a.ApprenticeshipLocations.Select(l => l.ApprenticeshipLocationType switch
                    {
                        ApprenticeshipLocationType.ClassroomBased => new CreateApprenticeshipQASubmissionApprenticeshipLocation(
                            new CreateApprenticeshipQASubmissionApprenticeshipClassroomLocation()
                            {
                                DeliveryModes = (ApprenticeshipDeliveryModes)l.DeliveryModes.Sum(),
                                Radius = l.Radius.Value,
                                VenueName = providerVenues.Single(v => v.Id == l.VenueId).VenueName
                            }),
                        ApprenticeshipLocationType.EmployerBased => new CreateApprenticeshipQASubmissionApprenticeshipLocation(
                            l.National == true ?
                                new CreateApprenticeshipQASubmissionApprenticeshipEmployerLocation(new National()) :
                                new CreateApprenticeshipQASubmissionApprenticeshipEmployerLocation(
                                    new CreateApprenticeshipQASubmissionApprenticeshipEmployerLocationRegions()
                                    {
                                        SubRegionIds = l.Regions.ToList()
                                    })),
                        _ => throw new NotSupportedException()
                    }).ToList()
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
