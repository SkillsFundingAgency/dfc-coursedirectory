﻿using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Validation;
using System.Linq;
using static Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider.FlowModel;
using Dfc.CourseDirectory.WebV2.Models;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    public class FlowModelInitializer
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ISqlQueryDispatcher _sqlDbQueryDispatcher;
        private readonly IStandardsAndFrameworksCache _iStandardandFromworkCache;

        public FlowModelInitializer(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher, ISqlQueryDispatcher sqlDbQueryDispatcher, IStandardsAndFrameworksCache iStandardandFromworkCache)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _sqlDbQueryDispatcher = sqlDbQueryDispatcher;
            _iStandardandFromworkCache = iStandardandFromworkCache;
        }

        public async Task<FlowModel> Initialize(Guid providerId)
        {
            var model = new FlowModel();
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetProviderById()
                {
                    ProviderId = providerId
                });


            var submission = await _sqlDbQueryDispatcher.ExecuteQuery(new GetLatestApprenticeshipQASubmissionForProvider()
            {
                ProviderId = providerId
            });

            var existingSubmission = submission.Match(_ => null,
                                                      match => match);

            if (existingSubmission != null)
            {
                var apprenticeship = existingSubmission.Apprenticeships.Single();
                var apprenticeshipId = apprenticeship.ApprenticeshipId;
                var cosmosApprenticeship = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetApprenticeshipsByIds() { ApprenticeshipIds = new Guid[] { apprenticeshipId } });

                if (cosmosApprenticeship.ContainsKey(apprenticeshipId))
                {
                    Standard standard = default;
                    Framework framework = default;

                    if(cosmosApprenticeship[apprenticeshipId].StandardCode.HasValue)
                        standard = await _iStandardandFromworkCache.GetStandard(cosmosApprenticeship[apprenticeshipId].StandardCode.Value, cosmosApprenticeship[apprenticeshipId].Version.Value);
                    
                    if(cosmosApprenticeship[apprenticeshipId].FrameworkCode.HasValue)
                        framework = await _iStandardandFromworkCache.GetFramework(cosmosApprenticeship[apprenticeshipId].FrameworkCode.Value, cosmosApprenticeship[apprenticeshipId].ProgType.Value, cosmosApprenticeship[apprenticeshipId].PathwayCode.Value);

                    model.ApprenticeshipWebsite = cosmosApprenticeship[apprenticeshipId].ContactWebsite;
                    model.ApprenticeshipStandardOrFramework = cosmosApprenticeship[apprenticeshipId].StandardCode.HasValue ?
                        new StandardOrFramework(
                            new Standard()
                            {
                                StandardCode = cosmosApprenticeship[apprenticeshipId].StandardCode.Value,
                                CosmosId = cosmosApprenticeship[apprenticeshipId].StandardId.Value,
                                Version = standard.Version,
                                StandardName = standard.StandardName,
                                NotionalNVQLevelv2 = cosmosApprenticeship[apprenticeshipId].NotionalNVQLevelv2,
                                OtherBodyApprovalRequired = standard.OtherBodyApprovalRequired
                            })
                        :
                        new StandardOrFramework(
                            new Framework()
                            {
                                FrameworkCode = cosmosApprenticeship[apprenticeshipId].FrameworkCode.Value,
                                CosmosId = cosmosApprenticeship[apprenticeshipId].FrameworkId.Value,
                                ProgType = cosmosApprenticeship[apprenticeshipId].ProgType.Value,
                                PathwayCode = cosmosApprenticeship[apprenticeshipId].PathwayCode.Value,
                                NasTitle = framework.NasTitle
                            });
                    model.ApprenticeshipId = apprenticeship.ApprenticeshipId;
                    model.ApprenticeshipMarketingInformation = apprenticeship.ApprenticeshipMarketingInformation;
                    model.ApprenticeshipContactTelephone = cosmosApprenticeship[apprenticeshipId].ContactTelephone;
                    model.ApprenticeshipContactEmail = cosmosApprenticeship[apprenticeshipId].ContactEmail;
                    model.ApprenticeshipContactWebsite = cosmosApprenticeship[apprenticeshipId].ContactWebsite;
                    model.ApprenticeshipLocationType = 
                        ((cosmosApprenticeship[apprenticeshipId].ApprenticeshipLocations.Any(l => l.VenueId.HasValue) ? ApprenticeshipLocationType.ClassroomBased : 0) |
                        (cosmosApprenticeship[apprenticeshipId].ApprenticeshipLocations.Any(l => l.National == true || (l.Regions?.Count() ?? 0) > 0) ? ApprenticeshipLocationType.EmployerBased : 0));
                    model.ApprenticeshipIsNational = cosmosApprenticeship[apprenticeshipId].ApprenticeshipLocations.Any(x => x.National == true);
                    model.ApprenticeshipLocationRegionIds = cosmosApprenticeship[apprenticeshipId].ApprenticeshipLocations?.SelectMany(x => x.Regions ?? new List<string>())?.ToList();

                    if (model.ApprenticeshipLocationType.Value.HasFlag(ApprenticeshipLocationType.ClassroomBased) || model.ApprenticeshipLocationType.Value.HasFlag(ApprenticeshipLocationType.ClassroomBasedAndEmployerBased))
                    {
                        var locations = cosmosApprenticeship[apprenticeshipId].ApprenticeshipLocations.Where(x => x.ApprenticeshipLocationType.HasFlag(ApprenticeshipLocationType.ClassroomBased));
                        model.ApprenticeshipClassroomLocations = locations.ToDictionary(x => x.Id, y => new ClassroomLocation()
                        {
                            VenueId = y.VenueId ?? Guid.Empty,
                            National = y.National ?? false,
                            Radius = y.Radius,
                            DeliveryModes = (ApprenticeshipDeliveryModes)y.DeliveryModes.Sum(x => x)
                        });
                    }
                }
            }

            if (!string.IsNullOrEmpty(provider.MarketingInformation))
            {
                model.SetProviderDetails(Html.SanitizeHtml(provider.MarketingInformation));
            }

            return model;
        }
    }
}
