using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetCourseUploadHandler : ISqlQueryHandler<GetCourseUpload, CourseUpload>
    {
        public Task<CourseUpload> Execute(SqlTransaction transaction, GetCourseUpload query)
        {
            var sql = $@"
SELECT CourseUploadId, ProviderId, UploadStatus, CreatedOn, CreatedByUserId,
ProcessingStartedOn, ProcessingCompletedOn, PublishedOn, AbandonedOn
FROM Pttcd.CourseUploads WITH (HOLDLOCK)
WHERE CourseUploadId = @CourseUploadId";

            return transaction.Connection.QuerySingleOrDefaultAsync<CourseUpload>(sql, new { query.CourseUploadId }, transaction);
        }
    }
}
