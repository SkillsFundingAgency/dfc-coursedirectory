using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetApprenticeshipUploadRowsToRevalidateHandler :
        ISqlQueryHandler<GetApprenticeshipUploadRowsToRevalidate, IReadOnlyCollection<ApprenticeshipUploadRow>>
    {
        public async Task<IReadOnlyCollection<ApprenticeshipUploadRow>> Execute(SqlTransaction transaction, GetApprenticeshipUploadRowsToRevalidate query)
        {
            var sql = $@"
DECLARE @LastVenueChangeForProvider DATETIME

SELECT @LastVenueChangeForProvider = MAX(ISNULL(v.UpdatedOn, v.CreatedOn))
FROM Pttcd.ApprenticeshipUploads au
JOIN Pttcd.Providers p ON au.ProviderId = p.ProviderId
JOIN Pttcd.Venues v ON p.Ukprn = v.ProviderUkprn
WHERE au.ApprenticeshipUploadId = @ApprenticeshipUploadId

SELECT
    RowNumber, ApprenticeshipUploadRowStatus, IsValid, Errors, LastUpdated, LastValidated, ApprenticeshipId, ApprenticeshipLocationId,
    StandardCode, StandardVersion, ApprenticeshipInformation, ApprenticeshipWebpage, ContactEmail, ContactPhone, ContactUrl,
    DeliveryMethod, aur.VenueName, YourVenueReference, Radius, DeliveryModes, NationalDelivery, SubRegions, aur.VenueId
FROM Pttcd.ApprenticeshipUploadRows aur
LEFT JOIN Pttcd.Venues v ON aur.VenueId = v.VenueId
WHERE aur.ApprenticeshipUploadId = @ApprenticeshipUploadId
AND aur.ApprenticeshipUploadRowStatus =  {(int)UploadRowStatus.Default}
AND aur.ResolvedDeliveryMethod = {(int)ApprenticeshipLocationType.ClassroomBased}
AND (
    -- Matched venue has been updated or deleted since we last validated row
    (aur.VenueId IS NOT NULL AND ISNULL(v.UpdatedOn, v.CreatedOn) > aur.LastValidated)

    OR

    -- Don't have a matched venue yet but a venue for the provider has been changed since last validated row
    (aur.VenueId IS NULL AND @LastVenueChangeForProvider IS NOT NULL AND @LastVenueChangeForProvider > aur.LastValidated)
)
ORDER BY aur.RowNumber";

            var results = (await transaction.Connection.QueryAsync<Result>(sql, new { query.ApprenticeshipUploadId }, transaction))
                .AsList();

            foreach (var row in results)
            {
                row.Errors = (row.ErrorList ?? string.Empty).Split(";", StringSplitOptions.RemoveEmptyEntries);
            }

            return results;
        }

        private class Result : ApprenticeshipUploadRow
        {
            public string ErrorList { get; set; }
            public UploadRowStatus ApprenticeshipUploadRowStatus { get; set; }
        }
    }
}
