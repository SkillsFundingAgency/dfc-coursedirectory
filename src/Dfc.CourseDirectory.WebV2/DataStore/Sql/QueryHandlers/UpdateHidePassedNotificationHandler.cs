using Dapper;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using OneOf;
using OneOf.Types;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.QueryHandlers
{
    public class UpdateHidePassedNotificationHandler : ISqlQueryHandler<UpdateHidePassedNotification, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(SqlTransaction transaction, UpdateHidePassedNotification query)
        {
            var sql = @"
UPDATE Pttcd.ApprenticeshipQASubmissions
SET HidePassedNotification = @HidePassedNotification
WHERE ApprenticeshipQASubmissionId = @ApprenticeshipQASubmissionId";

            var updated = await transaction.Connection.ExecuteAsync(sql, query, transaction);

            if (updated == 0)
            {
                return new NotFound();
            }
            else
            {
                return new Success();
            }
        }
    }
}