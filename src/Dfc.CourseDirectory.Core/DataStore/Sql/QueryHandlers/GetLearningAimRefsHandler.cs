using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLearningAimRefsHandler : ISqlQueryHandler<GetLearningAimRefs, IReadOnlyCollection<string>>
    {
        public async Task<IReadOnlyCollection<string>> Execute(SqlTransaction transaction, GetLearningAimRefs query)
        {
            var sql = @"
SELECT ld.LearnAimRef FROM LARS.LearningDelivery ld
JOIN @LearningAimRefs x ON ld.LearnAimRef = x.Value";

            var paramz = new
            {
                LearningAimRefs = TvpHelper.CreateUnicodeStringTable(query.LearningAimRefs)
            };

            return (await transaction.Connection.QueryAsync<string>(sql, paramz, transaction)).AsList();
        }
    }
}
