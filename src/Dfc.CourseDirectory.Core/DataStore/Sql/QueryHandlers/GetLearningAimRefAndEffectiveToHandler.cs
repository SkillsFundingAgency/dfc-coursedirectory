using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLearningAimRefAndEffectiveToHandler : ISqlQueryHandler<GetLearningAimRefAndEffectiveTo, Lars>
    {
        public async Task<Lars> Execute(SqlTransaction transaction, GetLearningAimRefAndEffectiveTo query)
        {
            var sql = @"SELECT LearnAimRef, EffectiveTo FROM LARS.LearningDelivery WHERE LearnAimRef=@LearningAimRef";

            var paramz = new
            {
                query.LearningAimRef
            };

            var result = await transaction.Connection.QuerySingleOrDefaultAsync<Lars>(sql, paramz, transaction);

            return result;
        }
    }
}
