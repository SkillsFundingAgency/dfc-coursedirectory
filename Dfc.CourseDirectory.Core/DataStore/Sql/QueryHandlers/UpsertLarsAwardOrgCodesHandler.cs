using System.Data.SqlClient;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertLarsAwardOrgCodesHandler : ISqlQueryHandler<UpsertLarsAwardOrgCodes, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, UpsertLarsAwardOrgCodes query)
        {
            await UpsertHelper.Upsert(
                transaction,
                query.Records,
                keyPropertyNames: new[] { nameof(UpsertLarsAwardOrgCodesRecord.AwardOrgCode) },
                "LARS.AwardOrgCode");

            return new None();
        }
    }
}
