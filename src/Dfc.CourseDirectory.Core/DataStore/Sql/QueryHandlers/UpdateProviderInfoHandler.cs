using System.Data.SqlClient;
using System.Net;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpdateProviderInfoHandler : ISqlQueryHandler<UpdateProviderInfo, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(
            SqlTransaction transaction,
            UpdateProviderInfo query)
        {
            var sql = $@"UPDATE [Pttcd].[Providers]
                         SET [MarketingInformation] = @MarketingInformation,
                            [UpdatedOn] = @UpdatedOn,
                            [UpdatedBy] = @UpdatedBy
                        WHERE [ProviderId] = @ProviderId ";


            var paramz = new
            {

                query.MarketingInformation,
                query.UpdatedOn,
                UpdatedBy = query.UpdatedBy.UserId,
                query.ProviderId
             };

            var updated = await transaction.Connection.ExecuteAsync(sql, paramz, transaction) == 1;

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
