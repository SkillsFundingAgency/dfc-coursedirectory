using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetTLevelDefinitionExemplarContentHandler :
        ISqlQueryHandler<GetTLevelDefinitionExemplarContent, TLevelDefinitionExemplarContent>
    {
        public Task<TLevelDefinitionExemplarContent> Execute(
            SqlTransaction transaction,
            GetTLevelDefinitionExemplarContent query)
        {
            var sql = @"
SELECT
    t.ExemplarWhoFor WhoFor,
    t.ExemplarEntryRequirements EntryRequirements,
    t.ExemplarWhatYoullLearn WhatYoullLearn,
    t.ExemplarHowYoullLearn HowYoullLearn,
    t.ExemplarHowYoullBeAssessed HowYoullBeAssessed,
    t.ExemplarWhatYouCanDoNext WhatYouCanDoNext
FROM Pttcd.TLevelDefinitions t
WHERE t.TLevelDefinitionId = @TLevelDefinitionId";

            return transaction.Connection.QuerySingleAsync<TLevelDefinitionExemplarContent>(
                sql,
                param: new { query.TLevelDefinitionId },
                transaction: transaction);
        }
    }
}
