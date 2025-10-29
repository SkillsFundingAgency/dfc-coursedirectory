using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2
{
    public class CommitSqlTransactionMiddleware
    {
        private readonly RequestDelegate _next;

        public CommitSqlTransactionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);

            var sqlTransactionMarker = context.RequestServices.GetRequiredService<SqlTransactionMarker>();
            if (sqlTransactionMarker.GotTransaction)
            {
                sqlTransactionMarker.Transaction.Commit();
                sqlTransactionMarker.OnTransactionCompleted();
            }
        }
    }
}
