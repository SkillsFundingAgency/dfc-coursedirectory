using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class DeleteCourseUploadRowGroupHandler : ISqlQueryHandler<DeleteCourseUploadRowGroup, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(SqlTransaction transaction, DeleteCourseUploadRowGroup query)
        {
            var sql = $@"
UPDATE Pttcd.CourseUploadRows SET CourseUploadRowStatus = {(int)UploadRowStatus.Deleted}
WHERE CourseUploadId = @CourseUploadId AND CourseId = @CourseId";

            var paramz = new
            {
                query.CourseUploadId,
                query.CourseId
            };

            var updated = await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            if (updated > 0)
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
