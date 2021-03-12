using System.Data.SqlClient;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertLarsStandardsHandler : ISqlQueryHandler<UpsertLarsStandards, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, UpsertLarsStandards query)
        {
            await UpsertHelper.Upsert(
                transaction,
                query.Records,
                keyPropertyNames: new[] { nameof(UpsertLarsStandardRecord.StandardCode), nameof(UpsertLarsStandardRecord.Version) },
                "Pttcd.Standards");

            return new None();
        }
    }
}
