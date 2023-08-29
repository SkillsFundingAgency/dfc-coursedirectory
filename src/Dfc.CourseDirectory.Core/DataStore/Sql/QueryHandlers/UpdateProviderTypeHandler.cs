using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using OneOf;
using OneOf.Types;
using System.Data.SqlClient;
using Dapper;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpdateProviderTypeHandler : ISqlQueryHandler<UpdateProviderType, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(
            SqlTransaction transaction,
            UpdateProviderType query)
        {
            var sql = $@"
UPDATE 
";


            var paramz = new
            {
                query.ProviderType
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
