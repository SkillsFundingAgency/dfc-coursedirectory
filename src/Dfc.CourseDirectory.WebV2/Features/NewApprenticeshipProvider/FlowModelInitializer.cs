using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    public class FlowModelInitializer
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ISqlQueryDispatcher _sqlDbQueryDispatcher;
        private readonly IStandardsAndFrameworksCache _standardsAndFrameworksCache;

        public FlowModelInitializer(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher, ISqlQueryDispatcher sqlDbQueryDispatcher, IStandardsAndFrameworksCache standardsAndFrameworksCache)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _sqlDbQueryDispatcher = sqlDbQueryDispatcher;
            _standardsAndFrameworksCache = standardsAndFrameworksCache;
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

                    if (cosmosApprenticeship[apprenticeshipId].StandardCode.HasValue)
                    {
                        standard = await _standardsAndFrameworksCache.GetStandard(cosmosApprenticeship[apprenticeshipId].StandardCode.Value, cosmosApprenticeship[apprenticeshipId].Version.Value);
                        model.ApprenticeshipStandardOrFramework = new StandardOrFramework(
                            new Standard()
                            {
                                StandardCode = cosmosApprenticeship[apprenticeshipId].StandardCode.Value,
                                CosmosId = cosmosApprenticeship[apprenticeshipId].StandardId.Value,
                                Version = standard.Version,
                                StandardName = standard.StandardName,
                                NotionalNVQLevelv2 = cosmosApprenticeship[apprenticeshipId].NotionalNVQLevelv2,
                                OtherBodyApprovalRequired = standard.OtherBodyApprovalRequired
                            });
                    }
                    else if (cosmosApprenticeship[apprenticeshipId].FrameworkCode.HasValue)
                    {
                        framework = await _standardsAndFrameworksCache.GetFramework(cosmosApprenticeship[apprenticeshipId].FrameworkCode.Value, cosmosApprenticeship[apprenticeshipId].ProgType.Value, cosmosApprenticeship[apprenticeshipId].PathwayCode.Value);
                        model.ApprenticeshipStandardOrFramework = new StandardOrFramework(
                            new Framework()
                            {
                                FrameworkCode = cosmosApprenticeship[apprenticeshipId].FrameworkCode.Value,
                                CosmosId = cosmosApprenticeship[apprenticeshipId].FrameworkId.Value,
                                ProgType = cosmosApprenticeship[apprenticeshipId].ProgType.Value,
                                PathwayCode = cosmosApprenticeship[apprenticeshipId].PathwayCode.Value,
                                NasTitle = framework.NasTitle
                            });
                    }

                    model.ApprenticeshipWebsite = cosmosApprenticeship[apprenticeshipId].ContactWebsite;
                    model.ApprenticeshipId = apprenticeship.ApprenticeshipId;
                    model.ApprenticeshipMarketingInformation = apprenticeship.ApprenticeshipMarketingInformation;
                    model.ApprenticeshipContactTelephone = cosmosApprenticeship[apprenticeshipId].ContactTelephone;
                    model.ApprenticeshipContactEmail = cosmosApprenticeship[apprenticeshipId].ContactEmail;
                    model.ApprenticeshipContactWebsite = cosmosApprenticeship[apprenticeshipId].ContactWebsite;
                    model.ApprenticeshipLocationType =
                        ((cosmosApprenticeship[apprenticeshipId].ApprenticeshipLocations.Any(l => l.VenueId.HasValue) ? ApprenticeshipLocationType.ClassroomBased : 0) |
                        (cosmosApprenticeship[apprenticeshipId].ApprenticeshipLocations.Any(l => l.National == true || (l.Regions?.Count() ?? 0) > 0) ? ApprenticeshipLocationType.EmployerBased : 0));
                    model.ApprenticeshipIsNational = cosmosApprenticeship[apprenticeshipId].ApprenticeshipLocations.Any(x => x.National == true);
                    model.ApprenticeshipLocationSubRegionIds = cosmosApprenticeship[apprenticeshipId].ApprenticeshipLocations?.SelectMany(x => x.Regions ?? new List<string>())?.ToList();

                    if (model.ApprenticeshipLocationType.Value.HasFlag(ApprenticeshipLocationType.ClassroomBased) || model.ApprenticeshipLocationType.Value.HasFlag(ApprenticeshipLocationType.ClassroomBasedAndEmployerBased))
                    {
                        var locations = cosmosApprenticeship[apprenticeshipId].ApprenticeshipLocations.Where(x => x.ApprenticeshipLocationType.HasFlag(ApprenticeshipLocationType.ClassroomBased));
                        model.ApprenticeshipClassroomLocations = locations.ToDictionary(x => x.Id, y => new FlowModel.ClassroomLocationEntry()
                        {
                            VenueId = y.VenueId ?? Guid.Empty,
                            Radius = y.Radius ?? 0,
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
