﻿using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class CreateVenueHandler : ISqlQueryHandler<CreateVenue, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, CreateVenue query)
        {
            var sql = $@"
DECLARE @ProviderUkprn INT

SELECT @ProviderUkprn = Ukprn FROM Pttcd.Providers
WHERE ProviderId = @ProviderId

INSERT INTO Pttcd.Venues (
    VenueId,
    ProviderId,
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
    @VenueId,
    @ProviderId,
    {(int)VenueStatus.Live},
    @CreatedOn,
    @CreatedByUserId,
    @CreatedOn,
    @CreatedByUserId,
    @VenueName,
    @ProviderUkprn,
    @ProviderVenueRef,
    @AddressLine1,
    @AddressLine2,
    @Town,
    @County,
    @Postcode,
    geography::Point(@Latitude, @Longitude, 4326),
    @Telephone,
    @Email,
    @Website
)";

            var paramz = new
            {
                query.VenueId,
                query.ProviderId,
                query.CreatedOn,
                CreatedByUserId = query.CreatedBy.UserId,
                VenueName = query.Name,
                query.ProviderVenueRef,
                query.AddressLine1,
                query.AddressLine2,
                query.Town,
                query.County,
                query.Postcode,
                query.Latitude,
                query.Longitude,
                query.Telephone,
                query.Email,
                query.Website
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new Success();
        }
    }
}
