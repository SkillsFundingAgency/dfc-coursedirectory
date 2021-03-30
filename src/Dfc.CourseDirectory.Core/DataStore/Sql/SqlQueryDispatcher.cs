using System;
using System.Collections.Generic;
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
                var wrapperHandlerType = typeof(TaskQueryHandler<,>).MakeGenericType(query.GetType(), typeof(T));
                var wrappedHandler = (TaskQueryHandler<T>)Activator.CreateInstance(
                    wrapperHandlerType,
                    handler);

                return await wrappedHandler.Execute(Transaction, query);
            }
            finally
            {
                _executeLock.Release();
            }
        }

        public virtual async IAsyncEnumerable<T> ExecuteQuery<T>(ISqlQuery<IAsyncEnumerable<T>> query)
        {
            var handlerType = typeof(ISqlAsyncEnumerableQueryHandler<,>).MakeGenericType(query.GetType(), typeof(T));
            var handler = _serviceProvider.GetRequiredService(handlerType);

            await _executeLock.WaitAsync();

            try
            {
                var wrapperHandlerType = typeof(AsyncEnumerableQueryHandler<,>).MakeGenericType(query.GetType(), typeof(T));
                var wrappedHandler = (AsyncEnumerableQueryHandler<T>)Activator.CreateInstance(
                    wrapperHandlerType,
                    handler);

                var result = wrappedHandler.Execute(Transaction, query);

                await foreach (var row in result)
                {
                    yield return row;
                }
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

        private abstract class AsyncEnumerableQueryHandler<T>
        {
            public abstract IAsyncEnumerable<T> Execute(SqlTransaction transaction, ISqlQuery<IAsyncEnumerable<T>> query);
        }

        private class AsyncEnumerableQueryHandler<TQuery, TResult> : AsyncEnumerableQueryHandler<TResult>
            where TQuery : ISqlQuery<IAsyncEnumerable<TResult>>
        {
            private readonly ISqlAsyncEnumerableQueryHandler<TQuery, TResult> _innerHandler;

            public AsyncEnumerableQueryHandler(ISqlAsyncEnumerableQueryHandler<TQuery, TResult> innerHandler)
            {
                _innerHandler = innerHandler;
            }

            public override IAsyncEnumerable<TResult> Execute(SqlTransaction transaction, ISqlQuery<IAsyncEnumerable<TResult>> query)
            {
                return _innerHandler.Execute(transaction, (TQuery)query);
            }
        }

        private abstract class TaskQueryHandler<T>
        {
            public abstract Task<T> Execute(SqlTransaction transaction, ISqlQuery<T> query);
        }

        private class TaskQueryHandler<TQuery, TResult> : TaskQueryHandler<TResult>
            where TQuery : ISqlQuery<TResult>
        {
            private readonly ISqlQueryHandler<TQuery, TResult> _innerHandler;

            public TaskQueryHandler(ISqlQueryHandler<TQuery, TResult> innerHandler)
            {
                _innerHandler = innerHandler;
            }

            public override Task<TResult> Execute(SqlTransaction transaction, ISqlQuery<TResult> query)
            {
                return _innerHandler.Execute(transaction, (TQuery)query);
            }
        }
    }
}
