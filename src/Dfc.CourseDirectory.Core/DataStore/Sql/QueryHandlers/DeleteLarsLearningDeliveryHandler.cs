using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class DeleteLarsLearningDeliveryHandler : ISqlQueryHandler<DeleteLarsLearningDelivery, string>
    {
        public async Task<string> Execute(SqlTransaction transaction, DeleteLarsLearningDelivery query)
        {
            var sql = @$"
                DELETE FROM LARS.LearningDelivery
                OUTPUT deleted.LearnAimRef
                WHERE LearnAimRef = @{nameof(query.LearnAimRef)}";

            return await transaction.Connection.QuerySingleAsync<string>(sql, query, transaction);
        }
    }
}