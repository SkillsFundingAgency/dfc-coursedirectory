using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpdateFindACourseIndexForVenuesHandler : ISqlQueryHandler<UpdateFindACourseIndexForVenues, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, UpdateFindACourseIndexForVenues query)
        {
            var sql = $@"
UPDATE Pttcd.FindACourseIndex SET
    LastSynced = @Now,
    VenueName = v.VenueName,
	VenueAddress = STUFF(
	    CONCAT(
		    NULLIF(', ' + v.AddressLine1, ', '),
		    NULLIF(', ' + v.AddressLine2, ', '),
		    NULLIF(', ' + v.Town, ', '),
		    NULLIF(', ' + v.County, ', '),
		    NULLIF(', ' + v.Postcode, ', ')),
	    1, 2, NULL),
    VenueTown = v.Town
FROM Pttcd.FindACourseIndex i
JOIN Pttcd.Venues v ON i.VenueId = v.VenueId
JOIN @VenueIds x ON v.VenueId = x.Id
WHERE Live = 1";

            var paramz = new
            {
                VenueIds = TvpHelper.CreateGuidIdTable(query.VenueIds),
                query.Now
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new Success();
        }
    }
}
