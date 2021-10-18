using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetAllApprenticeshipsHandler : ISqlQueryHandler<GetAllApprenticeships, IReadOnlyCollection<Apprenticeship>>
    {
        public async Task<IReadOnlyCollection<Apprenticeship>> Execute(SqlTransaction transaction, GetAllApprenticeships query)
        {
            var sql = $@"
SELECT
    a.ApprenticeshipId,
    a.CreatedOn,
    a.UpdatedOn,
    a.ProviderId,
    a.ProviderUkprn,
    a.MarketingInformation,
    a.ApprenticeshipWebsite,
    a.ContactTelephone,
    a.ContactEmail,
    a.ContactWebsite,
    s.StandardCode,
    s.Version,
    s.StandardName,
    s.NotionalEndLevel NotionalNVQLevelv2,
    s.OtherBodyApprovalRequired
FROM Pttcd.Apprenticeships a
JOIN Pttcd.Standards s ON a.StandardCode = s.StandardCode AND a.StandardVersion = s.Version
WHERE a.ApprenticeshipStatus = {(int)ApprenticeshipStatus.Live}

SELECT
    al.ApprenticeshipLocationId,
    al.ApprenticeshipId,
    al.ApprenticeshipLocationType,
    al.CreatedOn,
    al.UpdatedOn,
    al.Telephone,
    al.[National],
    al.Radius,
    al.DeliveryModes,
    al.VenueId,
    v.VenueId,
    v.ProviderId,
    v.ProviderUkprn,
    v.VenueName,
    v.ProviderVenueRef,
    v.AddressLine1,
    v.AddressLine2,
    v.Town,
    v.County,
    v.Postcode,
    v.Telephone,
    v.Email,
    v.Website,
    v.Position.Lat Latitude,
    v.Position.Long Longitude
FROM Pttcd.ApprenticeshipLocations al
JOIN Pttcd.Apprenticeships a ON al.ApprenticeshipId = a.ApprenticeshipId
LEFT JOIN Pttcd.Venues v ON al.VenueId = v.VenueId
WHERE al.ApprenticeshipLocationStatus = {(int)ApprenticeshipStatus.Live}
ORDER BY al.CreatedOn

SELECT r.ApprenticeshipLocationId, r.RegionId
FROM Pttcd.ApprenticeshipLocationRegions r
JOIN Pttcd.ApprenticeshipLocations al ON r.ApprenticeshipLocationId = al.ApprenticeshipLocationId
JOIN Pttcd.Apprenticeships a ON al.ApprenticeshipId = a.ApprenticeshipId";

            using var reader = await transaction.Connection.QueryMultipleAsync(sql, transaction: transaction);

            return await ApprenticeshipMappingHelper.MapApprenticeships(reader);
        }
    }
}
