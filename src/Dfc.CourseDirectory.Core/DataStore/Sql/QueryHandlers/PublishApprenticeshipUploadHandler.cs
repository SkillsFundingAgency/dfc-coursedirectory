using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class PublishApprenticeshipUploadHandler : ISqlQueryHandler<PublishApprenticeshipUpload, OneOf<NotFound, PublishApprenticeshipUploadResult>>
    {
        public async Task<OneOf<NotFound, PublishApprenticeshipUploadResult>> Execute(SqlTransaction transaction, PublishApprenticeshipUpload query)
        {
            var sql = $@"
UPDATE Pttcd.ApprenticeshipUploads
SET UploadStatus = {(int)UploadStatus.Published}, PublishedOn = @PublishedOn
WHERE ApprenticeshipUploadId = @ApprenticeshipUploadId

IF @@ROWCOUNT = 0
BEGIN
    SELECT 0 AS Status
    RETURN
END

DECLARE @ProviderUkprn INT,
        @ProviderId UNIQUEIDENTIFIER

SELECT @ProviderUkprn = Ukprn, @ProviderId = p.ProviderId FROM Pttcd.Providers p
JOIN Pttcd.ApprenticeshipUploads cu ON p.ProviderId = cu.ProviderId
WHERE cu.ApprenticeshipUploadId = @ApprenticeshipUploadId


-- Archive all existing apprenticeships

DECLARE @ApprenticeshipIds Pttcd.GuidIdTable

INSERT INTO @ApprenticeshipIds
SELECT a.ApprenticeshipId
FROM Pttcd.Apprenticeships a
WHERE a.ProviderId = @ProviderId
AND (a.ApprenticeshipStatus & (~ {(int)ApprenticeshipStatus.Archived})) <> 0

UPDATE Pttcd.Apprenticeships
SET ApprenticeshipStatus = {(int)ApprenticeshipStatus.Archived}
FROM Pttcd.Apprenticeships a
JOIN @ApprenticeshipIds x ON a.ApprenticeshipId = x.Id

UPDATE Pttcd.ApprenticeshipLocations 
SET ApprenticeshipLocationStatus = {(int)ApprenticeshipStatus.Archived}
FROM Pttcd.ApprenticeshipLocations al
JOIN @ApprenticeshipIds x ON al.ApprenticeshipId = x.Id
WHERE al.ApprenticeshipLocationStatus <> {(int)ApprenticeshipStatus.Archived}


-- Create new apprenticeships from the Apprenticeship Upload's rows

;WITH ApprenticeshipsCte AS (
    SELECT
        ApprenticeshipId,
        StandardCode,
        StandardVersion,
        ApprenticeshipInformation,
        ApprenticeshipWebpage,
        ContactPhone,
        ContactEmail,
        ContactUrl,
        ROW_NUMBER() OVER (PARTITION BY ApprenticeshipId ORDER BY RowNumber) AS GroupRowNumber
    FROM Pttcd.ApprenticeshipUploadRows
    WHERE ApprenticeshipUploadId = @ApprenticeshipUploadId
    AND ApprenticeshipUploadRowStatus = {(int)UploadRowStatus.Default}
)
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
    ApprenticeshipId,
    {(int)ApprenticeshipStatus.Live},
    @PublishedOn,
    @PublishedByUserId,
    @PublishedOn,
    @PublishedByUserId,
    @ProviderId,
    @ProviderUkprn,
    1,  --StandardCode,
    s.StandardName,
    a.StandardCode,
    a.StandardVersion,
    a.ApprenticeshipInformation,
    a.ApprenticeshipWebpage,
    a.ContactPhone,
    a.ContactEmail,
    a.ContactUrl
FROM ApprenticeshipsCte a
JOIN Pttcd.Standards s ON a.StandardCode = s.StandardCode and a.StandardVersion = s.Version
WHERE a.GroupRowNumber = 1

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
    DeliveryModes
)
SELECT
    ApprenticeshipLocationId,
    ApprenticeshipId,
    {(int)ApprenticeshipStatus.Live},
    @PublishedOn,
    @PublishedByUserId,
    @PublishedOn,
    @PublishedByUserId,
    NULL AS Telephone,
    VenueId,
    ResolvedNationalDelivery,
    ResolvedRadius,
    ResolvedDeliveryMethod,
    ResolvedDeliveryModes
FROM Pttcd.ApprenticeshipUploadRows
WHERE ApprenticeshipUploadId = @ApprenticeshipUploadId
AND ApprenticeshipUploadRowStatus = {(int)UploadRowStatus.Default}

INSERT INTO Pttcd.ApprenticeshipLocationRegions (ApprenticeshipLocationId, RegionId)
SELECT aur.ApprenticeshipLocationId, ar.RegionId
FROM Pttcd.ApprenticeshipUploadRows aur
JOIN Pttcd.ApprenticeshipUploadRowSubRegions ar ON aur.RowNumber = ar.RowNumber AND aur.ApprenticeshipUploadId = ar.ApprenticeshipUploadId
WHERE aur.ApprenticeshipUploadId = @ApprenticeshipUploadId
AND aur.ApprenticeshipUploadRowStatus = {(int)UploadRowStatus.Default}


SELECT 1 AS Status

SELECT COUNT(*) FROM Pttcd.ApprenticeshipUploadRows
WHERE ApprenticeshipUploadId = @ApprenticeshipUploadId
AND ApprenticeshipUploadRowStatus = {(int)UploadRowStatus.Default}

SELECT COUNT(DISTINCT ApprenticeshipId) FROM Pttcd.ApprenticeshipUploadRows
WHERE ApprenticeshipUploadId = @ApprenticeshipUploadId
AND ApprenticeshipUploadRowStatus = {(int)UploadRowStatus.Default}";

            var paramz = new
            {
                query.ApprenticeshipUploadId,
                query.PublishedOn,
                PublishedByUserId = query.PublishedBy.UserId
            };

            using var reader = await transaction.Connection.QueryMultipleAsync(sql, paramz, transaction);

            var status = await reader.ReadSingleAsync<int>();

            if (status == 1)
            {
                var publishedRowCount = await reader.ReadSingleAsync<int>();
                var publishedApprenticeshipsCount = await reader.ReadSingleAsync<int>();

                return new PublishApprenticeshipUploadResult()
                {
                    PublishedRowCount = publishedRowCount,
                    PublishedApprenticeshipsCount = publishedApprenticeshipsCount
                };
            }
            else
            {
                return new NotFound();
            }
        }
    }
}
