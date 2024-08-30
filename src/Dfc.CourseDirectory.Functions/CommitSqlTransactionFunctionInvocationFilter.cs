using Dfc.CourseDirectory.Core.DataStore.Sql;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Functions
{
    public class CommitSqlTransactionMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly FunctionInstanceServicesCatalog _functionInstanceServicesCatalog;

        public CommitSqlTransactionMiddleware(FunctionInstanceServicesCatalog functionInstanceServicesCatalog)
        {
            _functionInstanceServicesCatalog = functionInstanceServicesCatalog;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {            
            await next(context);
            
            var instanceServices = _functionInstanceServicesCatalog.GetFunctionServices(new Guid(context.InvocationId));

            var sqlTransactionMarker = instanceServices.GetRequiredService<SqlTransactionMarker>();
            if (sqlTransactionMarker.GotTransaction)
            {
                sqlTransactionMarker.Transaction.Commit();
                sqlTransactionMarker.OnTransactionCompleted();
            }
        }
    }
}
