using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    public class FlowModelInitializer
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ISqlQueryDispatcher _sqlDbQueryDispatcher;

        public FlowModelInitializer(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ISqlQueryDispatcher sqlDbQueryDispatcher)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _sqlDbQueryDispatcher = sqlDbQueryDispatcher;
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
                var apprenticeship = await _sqlDbQueryDispatcher.ExecuteQuery(new GetApprenticeship() { ApprenticeshipId = apprenticeshipId });

                model.SetApprenticeshipStandard(apprenticeship.Standard);

                model.SetApprenticeshipId(apprenticeship.ApprenticeshipId);

                model.SetApprenticeshipDetails(
                    apprenticeship.MarketingInformation,
                    apprenticeship.ApprenticeshipWebsite,
                    apprenticeship.ContactTelephone,
                    apprenticeship.ContactEmail,
                    apprenticeship.ContactWebsite);

                model.SetApprenticeshipLocationType(
                    ((apprenticeship.ApprenticeshipLocations.Any(l => l.Venue != null) ? ApprenticeshipLocationType.ClassroomBased : 0) |
                    (apprenticeship.ApprenticeshipLocations.Any(l => l.National == true || (l.SubRegionIds?.Count() ?? 0) > 0) ? ApprenticeshipLocationType.EmployerBased : 0)));

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
                            apprenticeship.ApprenticeshipLocations?.SelectMany(x => x.SubRegionIds ?? new List<string>())?.ToList());
                    }
                }

                if (model.ApprenticeshipLocationType.Value.HasFlag(ApprenticeshipLocationType.ClassroomBased))
                {
                    var locations = apprenticeship.ApprenticeshipLocations.Where(x => x.ApprenticeshipLocationType.HasFlag(ApprenticeshipLocationType.ClassroomBased));

                    foreach (var l in locations)
                    {
                        model.SetClassroomLocationForVenue(
                            l.Venue.VenueId,
                            l.Venue.VenueId,
                            l.Radius.Value,
                            l.DeliveryModes);
                    }
                }
            }

            return model;
        }
    }
}
