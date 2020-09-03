using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class SetProviderDisplayNameSourceHandler :
        ISqlQueryHandler<SetProviderDisplayNameSource, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(SqlTransaction transaction, SetProviderDisplayNameSource query)
        {
            var sql = "UPDATE Pttcd.Providers SET DisplayNameSource = @DisplayNameSource WHERE ProviderId = @ProviderId";

            var paramz = new
            {
                DisplayNameSource = query.DisplayNameSource,
                ProviderId = query.ProviderId
            };

            var updated = (await transaction.Connection.ExecuteAsync(sql, paramz, transaction)) == 1;

            if (updated)
            {
                return new Success();
            }
            else
            {
                return new NotFound();
            }
        }
    }
}
