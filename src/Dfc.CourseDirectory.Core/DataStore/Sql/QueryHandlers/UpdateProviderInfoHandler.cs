using System.Data.SqlClient;
using System.Net;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
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
            var sql = $@"
UPDATE 
";


            var paramz = new
            {

                query.MarketingInformation,
                query.UpdatedOn,
                query.UpdatedBy.UserId
             };

            var result = await transaction.Connection.QuerySingleAsync<Result>(sql, paramz, transaction);

            if (result == Result.Success)
            {
                return new Success();
            }
            else
            {
                return new NotFound();
            }
        }

        private enum Result { Success = 0, NotFound = 1 }
    }
}
