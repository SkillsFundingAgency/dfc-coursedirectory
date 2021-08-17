using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class CreateApprenticeshipHandler : ISqlQueryHandler<CreateApprenticeship, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, CreateApprenticeship query)
        {
            var sql = $@"
INSERT INTO Pttcd.Apprenticeships (
    ApprenticeshipId,
    ApprenticeshipStatus,
    CreatedOn,
    CreatedBy,
    UpdatedOn,
    UpdatedBy,
    ProviderId,
    ProviderUkprn,
    ApprenticeshipType,
    ApprenticeshipTitle,
    StandardCode,
    StandardVersion,
    MarketingInformation,
    ApprenticeshipWebsite,
    ContactTelephone,
    ContactEmail,
    ContactWebsite
)
SELECT
    @ApprenticeshipId,
    {(int)ApprenticeshipStatus.Live},
    @CreatedOn,
    @CreatedByUserId,
    @CreatedOn,
    @CreatedByUserId,
    p.ProviderId,
    p.Ukprn ProviderUkprn,
    {(int)ApprenticeshipType.Standard},
    s.StandardName,
    @StandardCode,
    @StandardVersion,
    @MarketingInformation,
    @ApprenticeshipWebsite,
    @ContactTelephone,
    @ContactEmail,
    @ContactWebsite
FROM Pttcd.Providers p
CROSS JOIN Pttcd.Standards s
WHERE p.ProviderId = @ProviderId
AND s.StandardCode = @StandardCode
AND s.Version = @StandardVersion

INSERT INTO Pttcd.ApprenticeshipLocations (
    ApprenticeshipLocationId,
    ApprenticeshipId,
    ApprenticeshipLocationStatus,
    CreatedOn,
    CreatedBy,
    UpdatedOn,
    UpdatedBy,
    Telephone,
    VenueId,
    [National],
    Radius,
    ApprenticeshipLocationType,
    Name,
    DeliveryModes
)
SELECT
    l.ApprenticeshipLocationId,
    @ApprenticeshipId,
    {(int)ApprenticeshipStatus.Live} ApprenticeshipLocationStatus,
    @CreatedOn,
    @CreatedByUserId,
    @CreatedOn,
    @CreatedByUserId,
    l.Telephone,
    l.VenueId,
    l.[National],
    l.Radius,
    l.ApprenticeshipLocationType,
    v.VenueName,
    l.DeliveryModes
FROM @ApprenticeshipLocations l
LEFT JOIN Pttcd.Venues v ON l.VenueId = v.VenueId

INSERT INTO Pttcd.ApprenticeshipLocationRegions (ApprenticeshipLocationId, RegionId)
SELECT ApprenticeshipLocationId, RegionId FROM @ApprenticeshipLocationSubRegions";

            var paramz = new
            {
                query.ApprenticeshipId,
                query.CreatedOn,
                CreatedByUserId = query.CreatedBy.UserId,
                query.ProviderId,
                query.Standard.StandardCode,
                StandardVersion = query.Standard.Version,
                query.MarketingInformation,
                query.ApprenticeshipWebsite,
                query.ContactTelephone,
                query.ContactEmail,
                query.ContactWebsite,
                ApprenticeshipLocations = TvpHelper.CreateApprenticeshipLocationsTable(
                    query.ApprenticeshipLocations.Select(l => (
                        l.ApprenticeshipLocationId,
                        l.Telephone,
                        l.VenueId,
                        l.National,
                        l.Radius,
                        l.ApprenticeshipLocationType,
                        l.DeliveryModes
                    ))),
                ApprenticeshipLocationSubRegions = TvpHelper.CreateApprenticeshipLocationSubRegionsTable(
                    from al in query.ApprenticeshipLocations
                    from subRegionId in al.SubRegionIds ?? Array.Empty<string>()
                    select (al.ApprenticeshipLocationId, subRegionId))
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new Success();
        }
    }
}
