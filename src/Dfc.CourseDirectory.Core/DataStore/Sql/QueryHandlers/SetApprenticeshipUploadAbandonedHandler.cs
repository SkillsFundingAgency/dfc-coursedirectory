using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class SetApprenticeshipUploadAbandonedHandler : ISqlQueryHandler<SetApprenticeshipUploadAbandoned, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(SqlTransaction transaction, SetApprenticeshipUploadAbandoned query)
        {
            var sql = $@"
UPDATE Pttcd.ApprenticeshipUploads
SET UploadStatus = {(int)UploadStatus.Abandoned}, AbandonedOn = @AbandonedOn
WHERE ApprenticeshipUploadId = @ApprenticeshipUploadId";

            var paramz = new
            {
                query.ApprenticeshipUploadId,
                query.AbandonedOn
            };

            var updated = await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            if (updated == 1)
            {
                return new Success();
            }
            else
            {
                return new NotFound();
            }
        }
    }
}
