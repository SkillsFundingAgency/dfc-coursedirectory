using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class PublishVenueUploadHandler : ISqlQueryHandler<PublishVenueUpload, OneOf<NotFound, PublishVenueUploadResult>>
    {
        private readonly ISqlQueryHandler<UpdateFindACourseIndexForVenues, Success> _updateFindACourseIndexHandler;

        public PublishVenueUploadHandler(ISqlQueryHandler<UpdateFindACourseIndexForVenues, Success> updateFindACourseIndexHandler)
        {
            _updateFindACourseIndexHandler = updateFindACourseIndexHandler;
        }

        public async Task<OneOf<NotFound, PublishVenueUploadResult>> Execute(SqlTransaction transaction, PublishVenueUpload query)
        {
            var sql = $@"
UPDATE Pttcd.VenueUploads
SET UploadStatus = {(int)UploadStatus.Published}, PublishedOn = @PublishedOn
WHERE VenueUploadId = @VenueUploadId

IF @@ROWCOUNT = 0
BEGIN
    SELECT 0 AS Status
    RETURN
END

DECLARE @ProviderUkprn INT

SELECT @ProviderUkprn = Ukprn FROM Pttcd.Providers p
JOIN Pttcd.VenueUploads vu ON p.ProviderId = vu.ProviderId
WHERE VenueUploadId = @VenueUploadId

MERGE Pttcd.Venues AS target
USING (
    SELECT r.*, pc.Position FROM Pttcd.VenueUploadRows r
    JOIN Pttcd.Postcodes pc ON CONVERT(VARCHAR(8), r.Postcode) = pc.Postcode
    WHERE r.VenueUploadId = @VenueUploadId
    AND r.VenueUploadRowStatus = {(int)UploadRowStatus.Default}
) AS source
ON target.VenueId = source.VenueId
WHEN MATCHED THEN UPDATE SET
    ProviderVenueRef = source.ProviderVenueRef,
    VenueName = source.VenueName,
    Email = source.Email,
    Telephone = source.Telephone,
    Website = source.Website,
    AddressLine1 = source.AddressLine1,
    AddressLine2 = source.AddressLine2,
    Town = source.Town,
    County = source.County,
    Postcode = source.Postcode,
    Position = source.Position,
    UpdatedOn = @PublishedOn,
    UpdatedBy = @PublishedByUserId
WHEN NOT MATCHED THEN INSERT (
    VenueId,
    VenueStatus,
    CreatedOn,
    CreatedBy,
    UpdatedOn,
    UpdatedBy,
    VenueName,
    ProviderUkprn,
    ProviderVenueRef,
    AddressLine1,
    AddressLine2,
    Town,
    County,
    Postcode,
    Position,
    Telephone,
    Email,
    Website
) VALUES (
    source.VenueId,
    {(int)VenueStatus.Live},
    @PublishedOn,
    @PublishedByUserId,
    @PublishedOn,
    @PublishedByUserId,
    source.VenueName,
    @ProviderUkprn,
    source.ProviderVenueRef,
    source.AddressLine1,
    source.AddressLine2,
    source.Town,
    source.County,
    source.Postcode,
    source.Position,
    source.Telephone,
    source.Email,
    source.Website
)
WHEN NOT MATCHED BY SOURCE AND target.ProviderUkprn = @ProviderUkprn AND target.VenueStatus = {(int)VenueStatus.Live} THEN UPDATE SET
    VenueStatus = {(int)VenueStatus.Archived}
;

SELECT 1 AS Status

SELECT r.VenueId FROM Pttcd.VenueUploadRows r
WHERE r.VenueUploadId = @VenueUploadId
AND r.VenueUploadRowStatus = {(int)UploadRowStatus.Default}
";

            var paramz = new
            {
                query.VenueUploadId,
                query.PublishedOn,
                PublishedByUserId = query.PublishedBy.UserId
            };

            using var reader = await transaction.Connection.QueryMultipleAsync(sql, paramz, transaction);

            var status = await reader.ReadSingleAsync<int>();

            if (status == 1)
            {
                var publishedVenueIds = (await reader.ReadAsync<Guid>()).AsList();

                await _updateFindACourseIndexHandler.Execute(
                    transaction,
                    new UpdateFindACourseIndexForVenues()
                    {
                        VenueIds = publishedVenueIds,
                        Now = query.PublishedOn
                    });

                return new PublishVenueUploadResult()
                {
                    PublishedCount = publishedVenueIds.Count
                };
            }
            else
            {
                return new NotFound();
            }
        }
    }
}
