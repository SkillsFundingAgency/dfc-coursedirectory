using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLearningDeliveriesHandler : ISqlQueryHandler<GetLearningDeliveries, IReadOnlyDictionary<string, LearningDelivery>>
    {
        public async Task<IReadOnlyDictionary<string, LearningDelivery>> Execute(SqlTransaction transaction, GetLearningDeliveries query)
        {
            var sql = @"
SELECT ld.LearnAimRef, ld.LearnAimRefTitle, ld.EffectiveTo, ld.NotionalNVQLevelv2, ld.AwardOrgCode, 
lart.LearnAimRefTypeDesc, ld.OperationalEndDate
FROM LARS.LearningDelivery ld
JOIN @LearningAimRefs x ON ld.LearnAimRef = x.Value
JOIN LARS.LearnAimRefType lart ON ld.LearnAimRefType = lart.LearnAimRefType";

            var paramz = new
            {
                LearningAimRefs = TvpHelper.CreateUnicodeStringTable(query.LearnAimRefs)
            };

            return (await transaction.Connection.QueryAsync<LearningDelivery>(sql, paramz, transaction)).ToDictionary(r => r.LearnAimRef, r => r);
        }
    }
}

