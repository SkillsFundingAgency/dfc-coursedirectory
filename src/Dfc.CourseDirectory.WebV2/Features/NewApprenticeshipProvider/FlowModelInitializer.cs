using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    public class FlowModelInitializer
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ISqlQueryDispatcher _sqlDbQueryDispatcher;
        private readonly IStandardsAndFrameworksCache _standardsAndFrameworksCache;

        public FlowModelInitializer(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ISqlQueryDispatcher sqlDbQueryDispatcher,
            IStandardsAndFrameworksCache standardsAndFrameworksCache)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _sqlDbQueryDispatcher = sqlDbQueryDispatcher;
            _standardsAndFrameworksCache = standardsAndFrameworksCache;
        }

        public async Task<FlowModel> Initialize(Guid providerId)
        {
            var model = new FlowModel();

            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new Core.DataStore.CosmosDb.Queries.GetProviderById()
                {
                    ProviderId = providerId
                });

            if (!string.IsNullOrEmpty(provider.MarketingInformation))
            {
                model.SetProviderDetails(Html.SanitizeHtml(provider.MarketingInformation));
            }

            var submission = await _sqlDbQueryDispatcher.ExecuteQuery(new GetLatestApprenticeshipQASubmissionForProvider()
            {
                ProviderId = providerId
            });

            if (submission != null)
            {
                var apprenticeshipId = submission.Apprenticeships.First().ApprenticeshipId;
                var apprenticeship = (await _cosmosDbQueryDispatcher.ExecuteQuery(
                    new GetApprenticeshipsByIds() { ApprenticeshipIds = new Guid[] { apprenticeshipId } }))[apprenticeshipId];

                StandardOrFramework standardOrFramework;

                if (apprenticeship.StandardCode.HasValue)
                {
                    var standard = await _standardsAndFrameworksCache.GetStandard(
                        apprenticeship.StandardCode.Value,
                        apprenticeship.Version.Value);

                    standardOrFramework = new StandardOrFramework(
                        new Standard()
                        {
                            StandardCode = apprenticeship.StandardCode.Value,
                            CosmosId = apprenticeship.StandardId.Value,
                            Version = standard.Version,
                            StandardName = standard.StandardName,
                            NotionalNVQLevelv2 = apprenticeship.NotionalNVQLevelv2,
                            OtherBodyApprovalRequired = standard.OtherBodyApprovalRequired
                        });
                }
                else  // apprenticeship.FrameworkCode.HasValue
                {
                    var framework = await _standardsAndFrameworksCache.GetFramework(
                        apprenticeship.FrameworkCode.Value,
                        apprenticeship.ProgType.Value,
                        apprenticeship.PathwayCode.Value);

                    standardOrFramework = new StandardOrFramework(
                        new Framework()
                        {
                            FrameworkCode = apprenticeship.FrameworkCode.Value,
                            CosmosId = apprenticeship.FrameworkId.Value,
                            ProgType = apprenticeship.ProgType.Value,
                            PathwayCode = apprenticeship.PathwayCode.Value,
                            NasTitle = framework.NasTitle
                        });
                }

                model.SetApprenticeshipStandardOrFramework(standardOrFramework);

                model.SetApprenticeshipId(apprenticeship.Id);

                model.SetApprenticeshipDetails(
                    apprenticeship.MarketingInformation,
                    apprenticeship.Url,
                    apprenticeship.ContactTelephone,
                    apprenticeship.ContactEmail,
                    apprenticeship.ContactWebsite);

                model.SetApprenticeshipLocationType(
                    ((apprenticeship.ApprenticeshipLocations.Any(l => l.VenueId.HasValue) ? ApprenticeshipLocationType.ClassroomBased : 0) |
                    (apprenticeship.ApprenticeshipLocations.Any(l => l.National == true || (l.Regions?.Count() ?? 0) > 0) ? ApprenticeshipLocationType.EmployerBased : 0)));

                if (model.ApprenticeshipLocationType.Value.HasFlag(ApprenticeshipLocationType.EmployerBased))
                {
                    var national = apprenticeship.ApprenticeshipLocations.Any(x => x.National == true);

                    if (national)
                    {
                        model.SetApprenticeshipIsNational(national);
                    }
                    else
                    {
                        model.SetApprenticeshipLocationRegionIds(
                            apprenticeship.ApprenticeshipLocations?.SelectMany(x => x.Regions ?? new List<string>())?.ToList());
                    }
                }

                if (model.ApprenticeshipLocationType.Value.HasFlag(ApprenticeshipLocationType.ClassroomBased))
                {
                    var locations = apprenticeship.ApprenticeshipLocations.Where(x => x.ApprenticeshipLocationType.HasFlag(ApprenticeshipLocationType.ClassroomBased));

                    foreach (var l in locations)
                    {
                        model.SetClassroomLocationForVenue(
                            l.VenueId.Value,
                            l.VenueId.Value,
                            l.Radius.Value,
                            l.DeliveryModes);
                    }
                }
            }

            return model;
        }
    }
}
