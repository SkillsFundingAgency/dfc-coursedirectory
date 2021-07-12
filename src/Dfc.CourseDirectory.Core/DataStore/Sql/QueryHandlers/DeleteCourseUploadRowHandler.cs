using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;


namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class DeleteCourseUploadRowHandler : ISqlQueryHandler<DeleteCourseUploadRow, OneOf<Success, NotFound>>
    {
        public async Task<OneOf<Success, NotFound>> Execute(SqlTransaction transaction, DeleteCourseUploadRow query)
        {
            var sql = $@"
UPDATE Pttcd.CourseUploadRows SET
    CourseUploadRowStatus = {(int)UploadRowStatus.Deleted}
WHERE CourseUploadId = @CourseUploadId
AND RowNumber = @RowNumber
AND CourseUploadRowStatus <> {(int)UploadRowStatus.Deleted}";

            var paramz = new
            {
                query.CourseUploadId,
                query.RowNumber,
            };

            var deleted = (await transaction.Connection.ExecuteAsync(sql, paramz, transaction)) == 1;

            if (deleted)
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
