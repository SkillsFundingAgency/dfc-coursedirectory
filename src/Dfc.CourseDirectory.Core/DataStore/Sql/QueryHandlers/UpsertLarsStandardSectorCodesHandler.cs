using System.Data.SqlClient;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertLarsStandardSectorCodesHandler : ISqlQueryHandler<UpsertLarsStandardSectorCodes, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, UpsertLarsStandardSectorCodes query)
        {
            await UpsertHelper.Upsert(
                transaction,
                query.Records,
                keyPropertyNames: new[] { nameof(UpsertLarsStandardSectorCodeRecord.StandardSectorCode) },
                "Pttcd.StandardSectorCodes");

            return new None();
        }
    }
}
