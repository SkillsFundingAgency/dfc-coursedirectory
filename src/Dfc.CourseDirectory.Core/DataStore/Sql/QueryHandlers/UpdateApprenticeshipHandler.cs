using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpdateApprenticeshipHandler : ISqlQueryHandler<UpdateApprenticeship, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(SqlTransaction transaction, UpdateApprenticeship query)
        {
            var sql = $@"
UPDATE Pttcd.Apprenticeships SET
    UpdatedOn = @UpdatedOn,
    UpdatedBy = @UpdatedByUserId,
    ApprenticeshipTitle = s.StandardName,
    StandardCode = @StandardCode,
    StandardVersion = @StandardVersion,
    MarketingInformation = @MarketingInformation,
    ApprenticeshipWebsite = @ApprenticeshipWebsite,
    ContactTelephone = @ContactTelephone,
    ContactEmail = @ContactEmail,
    ContactWebsite = @ContactWebsite
FROM Pttcd.Apprenticeships a
CROSS JOIN Pttcd.Standards s
WHERE a.ApprenticeshipId = @ApprenticeshipId
AND a.ApprenticeshipStatus IN ({(int)ApprenticeshipStatus.Live}, {(int)ApprenticeshipStatus.Pending})
AND s.StandardCode = @StandardCode
AND s.Version = @StandardVersion

IF @@ROWCOUNT = 0
BEGIN
    SELECT 1 AS Result
    RETURN
END

MERGE Pttcd.ApprenticeshipLocations AS target
USING (
    SELECT v.VenueName Name, l.* FROM @ApprenticeshipLocations l
    LEFT JOIN Pttcd.Venues v ON l.VenueId = v.VenueId
) AS source
ON target.ApprenticeshipLocationId = source.ApprenticeshipLocationId
WHEN MATCHED THEN UPDATE SET
    UpdatedOn = @UpdatedOn,
    UpdatedBy = @UpdatedByUserId,
    Telephone = source.Telephone,
    VenueId = source.VenueId,
    [National] = source.[National],
    Radius = source.Radius,
    ApprenticeshipLocationType = source.ApprenticeshipLocationType,
    Name = source.Name,
    DeliveryModes = source.DeliveryModes
WHEN NOT MATCHED THEN
    INSERT (
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
    VALUES (
        source.ApprenticeshipLocationId,
        @ApprenticeshipId,
        {(int)ApprenticeshipStatus.Live},
        @UpdatedOn,
        @UpdatedByUserId,
        @UpdatedOn,
        @UpdatedByUserId,
        source.Telephone,
        source.VenueId,
        source.[National],
        source.Radius,
        source.ApprenticeshipLocationType,
        source.Name,
        source.DeliveryModes
    )
WHEN NOT MATCHED BY SOURCE AND target.ApprenticeshipId = @ApprenticeshipId THEN
    UPDATE SET ApprenticeshipLocationStatus = {(int)ApprenticeshipStatus.Archived};

MERGE Pttcd.ApprenticeshipLocationRegions AS target
USING (SELECT ApprenticeshipLocationId, RegionId FROM @ApprenticeshipLocationSubRegions) AS source
ON target.ApprenticeshipLocationId = source.ApprenticeshipLocationId AND target.RegionId = source.RegionId
WHEN NOT MATCHED THEN INSERT (ApprenticeshipLocationId, RegionId) VALUES (source.ApprenticeshipLocationId, source.RegionId)
WHEN NOT MATCHED BY SOURCE AND target.ApprenticeshipLocationId IN (SELECT TOP 1 ApprenticeshipLocationId FROM @ApprenticeshipLocationSubRegions) THEN DELETE;

SELECT 0 AS Result";

            var paramz = new
            {
                query.ApprenticeshipId,
                query.UpdatedOn,
                UpdatedByUserId = query.UpdatedBy.UserId,
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

            var result = await transaction.Connection.QuerySingleAsync<Result>(sql, paramz, transaction);

            if (result == Result.Success)
            {
                return new Success();
            }
            else
            {
                return new NotFound();
            }
        }

        private enum Result { Success = 0, NotFound = 1 }
    }
}
