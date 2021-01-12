using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Core.DataStore.Sql
{
    public class SqlQueryDispatcher : ISqlQueryDispatcher
    {
        private const IsolationLevel DefaultIsolationLevel = IsolationLevel.Snapshot;

        private readonly IServiceProvider _serviceProvider;
        private readonly SemaphoreSlim _executeLock = new SemaphoreSlim(1, 1);
        private SqlTransaction _transaction;
        private readonly object _transactionLock = new object();

        public SqlQueryDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public SqlTransaction Transaction
        {
            get
            {
                EnsureTransaction();
                return _transaction;
            }
        }

        public virtual void CreateTransaction(IsolationLevel isolationLevel)
        {
            lock (_transactionLock)
            {
                if (_transaction != null)
                {
                    throw new InvalidOperationException("Transaction has already been created.");
                }

                CreateTransactionCore(isolationLevel);
            }
        }

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

        private void CreateTransactionCore(IsolationLevel isolationLevel)
        {
            // Check we're inside the lock already
            Debug.Assert(Monitor.IsEntered(_transactionLock));

            var connection = _serviceProvider.GetRequiredService<SqlConnection>();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            _transaction = connection.BeginTransaction(isolationLevel);

            var marker = _serviceProvider.GetRequiredService<SqlTransactionMarker>();
            marker.OnTransactionCreated(Transaction);
        }

        private void EnsureTransaction()
        {
            lock (_transactionLock)
            {
                if (_transaction == null)
                {
                    CreateTransactionCore(DefaultIsolationLevel);
                }
            }
        }
    }
}
