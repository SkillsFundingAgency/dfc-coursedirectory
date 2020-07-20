using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertVenuesHandler : ISqlQueryHandler<UpsertVenues, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, UpsertVenues query)
        {
            var createTableSql = @"
CREATE TABLE #Venues (
    VenueId UNIQUEIDENTIFIER,
	VenueStatus TINYINT,
	CreatedOn DATETIME,
	CreatedBy NVARCHAR(MAX),
	UpdatedOn DATETIME,
	UpdatedBy NVARCHAR(MAX),
	VenueName NVARCHAR(MAX),
	ProviderUkprn INT,
	TribalProviderId INT,
	TribalVenueId INT,
	ProviderVenueRef NVARCHAR(MAX),
	AddressLine1 NVARCHAR(MAX),
	AddressLine2 NVARCHAR(MAX),
	Town NVARCHAR(MAX),
	County NVARCHAR(MAX),
	Postcode NVARCHAR(MAX),
	Latitude FLOAT,
	Longitude FLOAT,
	Telephone NVARCHAR(MAX),
	Email NVARCHAR(MAX),
	Website NVARCHAR(MAX)
)";

            await transaction.Connection.ExecuteAsync(createTableSql, transaction: transaction);

            await BulkCopyHelper.WriteRecords(
                query.Records.Select(r => new
                {
                    r.VenueId,
                    r.VenueStatus,
                    r.CreatedOn,
                    r.CreatedBy,
                    r.UpdatedOn,
                    r.UpdatedBy,
                    r.VenueName,
                    r.ProviderUkprn,
                    r.TribalVenueId,
                    r.ProviderVenueRef,
                    r.AddressLine1,
                    r.AddressLine2,
                    r.Town,
                    r.County,
                    r.Postcode,
                    r.Position.Latitude,
                    r.Position.Longitude,
                    r.Telephone,
                    r.Email,
                    r.Website
                }),
                tableName: "#Venues",
                transaction);

            var sql = @"
MERGE Pttcd.Venues AS target
USING (
    SELECT
        VenueId,
        VenueStatus,
        CreatedOn,
        CreatedBy,
        UpdatedOn,
        UpdatedBy,
        VenueName,
        ProviderUkprn,
        TribalVenueId,
        ProviderVenueRef,
        AddressLine1,
        AddressLine2,
        Town,
        County,
        Postcode,
        geography::Point(Latitude, Longitude, 4326) Position,
        Telephone,
        Email,
        Website
    FROM #Venues
) AS source
ON target.VenueId = source.VenueId
WHEN NOT MATCHED THEN
    INSERT (
        VenueId,
        VenueStatus,
        CreatedOn,
        CreatedBy,
        UpdatedOn,
        UpdatedBy,
        VenueName,
        ProviderUkprn,
        TribalVenueId,
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
        source.VenueStatus,
        source.CreatedOn,
        source.CreatedBy,
        source.UpdatedOn,
        source.UpdatedBy,
        source.VenueName,
        source.ProviderUkprn,
        source.TribalVenueId,
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
WHEN MATCHED THEN
    UPDATE SET
        VenueStatus = source.VenueStatus,
        CreatedOn = source.CreatedOn,
        CreatedBy = source.CreatedBy,
        UpdatedOn = source.UpdatedOn,
        UpdatedBy = source.UpdatedBy,
        VenueName = source.VenueName,
        ProviderUkprn = source.ProviderUkprn,
        TribalVenueId = source.TribalVenueId,
        ProviderVenueRef = source.ProviderVenueRef,
        AddressLine1 = source.AddressLine1,
        AddressLine2 = source.AddressLine2,
        Town = source.Town,
        County = source.County,
        Postcode = source.Postcode,
        Position = source.Position,
        Telephone = source.Telephone,
        Email = source.Email,
        Website = source.Website;";

            await transaction.Connection.ExecuteAsync(sql, transaction: transaction);

            return new None();
        }
    }
}
