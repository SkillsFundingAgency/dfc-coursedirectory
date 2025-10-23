using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpdateVenueHandler : ISqlQueryHandler<UpdateVenue, OneOf<NotFound, Success>>
    {
        private readonly ISqlQueryHandler<UpdateFindACourseIndexForVenues, Success> _updateFindACourseIndexHandler;

        public UpdateVenueHandler(ISqlQueryHandler<UpdateFindACourseIndexForVenues, Success> updateFindACourseIndexHandler)
        {
            _updateFindACourseIndexHandler = updateFindACourseIndexHandler;
        }

        public async Task<OneOf<NotFound, Success>> Execute(SqlTransaction transaction, UpdateVenue query)
        {
            var sql = $@"
UPDATE Pttcd.Venues SET
    ProviderVenueRef = @ProviderVenueRef,
    VenueName = @Name,
    Email = @Email,
    Telephone = @Telephone,
    Website = @Website,
    AddressLine1 = @AddressLine1,
    AddressLine2 = @AddressLine2,
    Town = @Town,
    County = @County,
    Postcode = @Postcode,
    Position = geography::Point(@Latitude, @Longitude, 4326),
    UpdatedOn = @UpdatedOn,
    UpdatedBy = @UpdatedByUserId
WHERE VenueId = @VenueId
AND VenueStatus = {(int)VenueStatus.Live}";

            var paramz = new
            {
                query.VenueId,
                query.ProviderVenueRef,
                query.Name,
                query.Email,
                query.Telephone,
                query.Website,
                query.AddressLine1,
                query.AddressLine2,
                query.Town,
                query.County,
                query.Postcode,
                query.Latitude,
                query.Longitude,
                query.UpdatedOn,
                UpdatedByUserId = query.UpdatedBy.UserId
            };

            var updated = (await transaction.Connection.ExecuteAsync(sql, paramz, transaction)) == 1;

            if (updated)
            {
                await _updateFindACourseIndexHandler.Execute(
                    transaction,
                    new UpdateFindACourseIndexForVenues()
                    {
                        VenueIds = new[] { query.VenueId },
                        Now = query.UpdatedOn
                    });

                return new Success();
            }
            else
            {
                return new NotFound();
            }
        }
    }
}
