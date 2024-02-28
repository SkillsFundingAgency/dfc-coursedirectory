using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class CreateCourseUploadHandler : ISqlQueryHandler<CreateCourseUpload, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, CreateCourseUpload query)
        {
            var sql = $@"
                INSERT INTO Pttcd.CourseUploads (
                    CourseUploadId,
                    ProviderId,
                    UploadStatus,
                    CreatedOn,
                    CreatedByUserId,
                    IsNonLars
                ) VALUES (
                    @CourseUploadId,
                    @ProviderId,
                    {(int)UploadStatus.Created},
                    @CreatedOn,
                    @CreatedByUserId,
                    @IsNonLars
                )";

            var paramz = new
            {
                query.CourseUploadId,
                query.ProviderId,
                query.CreatedOn,
                CreatedByUserId = query.CreatedBy.UserId,
                query.IsNonLars
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new Success();
        }
    }
}
