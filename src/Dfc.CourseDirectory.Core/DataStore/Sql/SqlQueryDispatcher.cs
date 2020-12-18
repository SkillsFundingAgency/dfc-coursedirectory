using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Core.DataStore.Sql
{
    public class SqlQueryDispatcher : ISqlQueryDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SemaphoreSlim _executeLock = new SemaphoreSlim(1, 1);

        public SqlQueryDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public SqlTransaction Transaction => _serviceProvider.GetRequiredService<SqlTransaction>();

        public virtual async Task<T> ExecuteQuery<T>(ISqlQuery<T> query)
        {
            var handlerType = typeof(ISqlQueryHandler<,>).MakeGenericType(query.GetType(), typeof(T));
            var handler = _serviceProvider.GetRequiredService(handlerType);

            await _executeLock.WaitAsync();

            try
            {
                // TODO We could make this waaay more efficient
                var result = await (Task<T>)handlerType.GetMethod("Execute").Invoke(
                    handler,
                    new object[] { Transaction, query });

                return result;
            }
            finally
            {
                _executeLock.Release();
            }
        }
    }
}
