using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class DeleteLarsLearningDeliveryHandler : ISqlQueryHandler<DeleteLarsLearningDelivery, OneOf<string, None>>
    {
        public async Task<OneOf<string, None>> Execute(SqlTransaction transaction, DeleteLarsLearningDelivery query)
        {
            var sql = @$"
                DELETE FROM LARS.LearningDelivery
                OUTPUT deleted.LearnAimRef
                WHERE LearnAimRef = @{nameof(query.LearnAimRef)}";

            var result = await transaction.Connection.QuerySingleOrDefaultAsync<string>(sql, query, transaction);

            if (string.IsNullOrWhiteSpace(result))
            {
                return new None();
            }

            return result;
        }
    }
}