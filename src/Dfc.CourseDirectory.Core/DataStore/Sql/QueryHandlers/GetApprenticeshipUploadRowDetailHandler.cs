using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetApprenticeshipUploadRowDetailHandler : ISqlQueryHandler<GetApprenticeshipUploadRowDetail, ApprenticeshipUploadRow>
    {
        public async Task<ApprenticeshipUploadRow> Execute(
            SqlTransaction transaction,
            GetApprenticeshipUploadRowDetail query)
        {
            var sql = $@"
SELECT
    RowNumber, IsValid, Errors AS ErrorList, ApprenticeshipId, LastUpdated, LastValidated,
    StandardCode, StandardVersion, ApprenticeshipInformation, ApprenticeshipWebpage, ContactEmail,
    ContactPhone, ContactUrl, DeliveryMethod, VenueName, YourVenueReference, Radius, DeliveryModes, NationalDelivery, SubRegions, VenueId
FROM Pttcd.ApprenticeshipUploadRows
WHERE ApprenticeshipUploadId = @ApprenticeshipUploadId AND RowNumber = @RowNumber
AND ApprenticeshipUploadRowStatus = {(int)UploadRowStatus.Default}
ORDER BY RowNumber
";

            var paramz = new
            {
                query.ApprenticeshipUploadId,
                query.RowNumber
            };

            using (var reader = await transaction.Connection.QueryMultipleAsync(sql, paramz, transaction))
            {
                var result = await reader.ReadSingleOrDefaultAsync<ApprenticeshipUploadRow>();

                if (result == null)
                {
                    return null;
                }

                return result;
            }
        }
    }
}
