﻿using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class DeleteTLevelHandler : ISqlQueryHandler<DeleteTLevel, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(SqlTransaction transaction, DeleteTLevel query)
        {
            var sql = @"
UPDATE Pttcd.TLevels SET
    TLevelStatus = @DeletedTLevelStatus,
    DeletedOn = @DeletedOn,
    DeletedByUserId = @DeletedByUserId
WHERE TLevelId = @TLevelId
AND TLevelStatus = @LiveTLevelStatus

DECLARE @TLevelIds Pttcd.GuidIdTable

INSERT INTO @TLevelIds VALUES (@TLevelId)

EXEC Pttcd.RefreshFindACourseIndexForTLevels @TLevelIds, @DeletedOn";

            var deleted = (await transaction.Connection.ExecuteAsync(
                sql,
                new
                {
                    query.TLevelId,
                    query.DeletedOn,
                    DeletedByUserId = query.DeletedBy.UserId,
                    DeletedTLevelStatus = TLevelStatus.Deleted,
                    LiveTLevelStatus = TLevelStatus.Live
                },
                transaction: transaction)) == 1;

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
