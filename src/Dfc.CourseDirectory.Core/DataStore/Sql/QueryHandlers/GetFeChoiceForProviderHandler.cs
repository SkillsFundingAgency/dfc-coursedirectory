using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using System.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetFeChoiceForProviderHandler : ISqlQueryHandler<GetFeChoiceForProvider, FeChoice>
    {
        public async Task<FeChoice> Execute(SqlTransaction transaction, GetFeChoiceForProvider query)
        {
            var paramz = new { query.ProviderUkprn };
            var sql = @"SELECT ID,Ukprn,LearnerSatisfaction,EmployerSatisfaction,CreatedDateTimeUtc,CreatedOn,CreatedBy,LastUpdatedBy,LastUpdatedOn FROM [Pttcd].[FeChoices] WHERE Ukprn = @ProviderUkprn";
            return (await transaction.Connection.QueryAsync<FeChoice>(sql, paramz, transaction)).FirstOrDefault();
        }
    }
}
