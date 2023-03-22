using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;
using System.Threading.Tasks;
using Dapper;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class MarkFindACourseIndexNotLiveForDeactiveProviderHandler : ISqlQueryHandler<MarkFindACourseIndexNotLiveForDeactiveProvider, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, MarkFindACourseIndexNotLiveForDeactiveProvider query)
        {
            var sql = $@"
update Pttcd.FindACourseIndex
set Live=0, UpdatedOn=GETDATE()
Where ProviderId = @ProviderId";

            var paramz = new
            {
                query.ProviderId
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new Success();
        }
    }
}

