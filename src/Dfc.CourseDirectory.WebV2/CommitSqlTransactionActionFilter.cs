using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2
{
    public class CommitSqlTransactionActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            var sqlTransactionMarker = context.HttpContext.RequestServices.GetRequiredService<SqlTransactionMarker>();
            if (sqlTransactionMarker.GotTransaction && !context.Canceled)
            {
                sqlTransactionMarker.Transaction.Commit();
                sqlTransactionMarker.OnTransactionCompleted();
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }
    }
}
