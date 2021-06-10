using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class PublishCourseUploadHandler : ISqlQueryHandler<PublishCourseUpload, OneOf<NotFound, PublishCourseUploadResult>>
    {
        public async Task<OneOf<NotFound, PublishCourseUploadResult>> Execute(SqlTransaction transaction, PublishCourseUpload query)
        {
            var sql = $@"
UPDATE Pttcd.CourseUploads
SET UploadStatus = {(int)UploadStatus.Published}, PublishedOn = @PublishedOn
WHERE CourseUploadId = @CourseUploadId

IF @@ROWCOUNT = 0
BEGIN
    SELECT 0 AS Status
    RETURN
END

SELECT 1 AS Status

SELECT r.CourseId FROM Pttcd.CourseUploadRows r
WHERE r.CourseUploadId = @CourseUploadId
AND r.CourseUploadRowStatus = {(int)UploadRowStatus.Default}
";

            var paramz = new
            {
                query.CourseUploadId,
                query.PublishedOn,
                PublishedByUserId = query.PublishedBy.UserId
            };

            using var reader = await transaction.Connection.QueryMultipleAsync(sql, paramz, transaction);

            var status = await reader.ReadSingleAsync<int>();

            if (status == 1)
            {
                var publishedCourseIds = (await reader.ReadAsync<Guid>()).AsList();

                return new PublishCourseUploadResult()
                {
                    PublishedCount = publishedCourseIds.Count
                };
            }
            else
            {
                return new NotFound();
            }
        }
    }
}
