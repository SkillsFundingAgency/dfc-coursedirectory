using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLearningDeliveriesHandler : ISqlQueryHandler<GetLearningDeliveries, IReadOnlyCollection<LearningDelivery>>
    {
        public async Task<IReadOnlyCollection<LearningDelivery>> Execute(SqlTransaction transaction, GetLearningDeliveries query)
        {
            var sql = @"
SELECT ld.LearnAimRef, ld.EffectiveTo FROM LARS.LearningDelivery ld
JOIN @LearningAimRefs x ON ld.LearnAimRef = x.Value";

            var paramz = new
            {
                LearningAimRefs = TvpHelper.CreateUnicodeStringTable(query.LearningAimRefs)
            };

            return (await transaction.Connection.QueryAsync<LearningDelivery>(sql, paramz, transaction)).AsList();
        }
    }
}

