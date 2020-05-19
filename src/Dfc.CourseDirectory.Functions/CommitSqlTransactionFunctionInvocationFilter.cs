using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Functions
{
#pragma warning disable CS0618 // Type or member is obsolete
    // IFunctionInvocationFilter is not obsolete just not complete - https://github.com/Azure/azure-webjobs-sdk/issues/1284
    public class CommitSqlTransactionFunctionInvocationFilter : IFunctionInvocationFilter
    {
        private readonly FunctionInstanceServicesCatalog _functionInstanceServicesCatalog;

        public CommitSqlTransactionFunctionInvocationFilter(FunctionInstanceServicesCatalog functionInstanceServicesCatalog)
        {
            _functionInstanceServicesCatalog = functionInstanceServicesCatalog;
        }

        public Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        {
            var instanceServices = _functionInstanceServicesCatalog.GetFunctionServices(executedContext.FunctionInstanceId);

            var sqlTransactionMarker = instanceServices.GetRequiredService<SqlTransactionMarker>();
            if (sqlTransactionMarker.GotTransaction)
            {
                sqlTransactionMarker.Transaction.Commit();
                sqlTransactionMarker.OnTransactionCompleted();
            }

            return Task.CompletedTask;
        }

        public Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
