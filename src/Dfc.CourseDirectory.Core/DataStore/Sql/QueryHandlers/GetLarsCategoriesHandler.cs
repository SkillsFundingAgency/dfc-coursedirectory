using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLarsCategoriesHandler : ISqlQueryHandler<GetLarsCategories, IReadOnlyCollection<LarsCategory>>
    {
        public async Task<IReadOnlyCollection<LarsCategory>> Execute(SqlTransaction transaction, GetLarsCategories query)
        {
            var sql = $@"SELECT [LearnAimRef],[CategoryRef]
  FROM [LARS].[LearningDeliveryCategory]
  WHERE LearnAimRef = @LearnAimRef";

            var param = new
            {
                LearnAimRef = query.LearnAimRef
            };

            return (await transaction.Connection.QueryAsync<LarsCategory>(sql, param, transaction))
                .AsList();
        }
    }
}
