using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Core.DataStore.Sql
{
    public class ServiceProviderSqlDispatcherFactory : ISqlQueryDispatcherFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderSqlDispatcherFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ISqlQueryDispatcher CreateDispatcher(IsolationLevel isolationLevel = IsolationLevel.Snapshot)
        {
            var connection = _serviceProvider.GetRequiredService<SqlConnection>();
            
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            var transaction = connection.BeginTransaction(isolationLevel);

            return new SqlQueryDispatcher(_serviceProvider, transaction);
        }

        private class SqlQueryDispatcher : ISqlQueryDispatcher, IDisposable
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly SemaphoreSlim _executeLock = new SemaphoreSlim(1, 1);
            private readonly SqlConnection _connection;
            private List<Action> _postCommitActions;

            internal SqlQueryDispatcher(IServiceProvider serviceProvider, SqlTransaction transaction)
            {
                _serviceProvider = serviceProvider;
                Transaction = transaction;
                _connection = transaction.Connection;
            }

            public SqlTransaction Transaction { get; }

            public async Task Commit()
            {
                await Transaction.CommitAsync();

                if (_postCommitActions != null)
                {
                    foreach (var action in _postCommitActions)
                    {
                        action();
                    }
                }
            }

            public void Dispose()
            {
                Transaction.Dispose();
                _connection.Dispose();
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

            public void RegisterPostCommitAction(Action action)
            {
                if (action is null)
                {
                    throw new ArgumentNullException(nameof(action));
                }

                _postCommitActions ??= new List<Action>();
                _postCommitActions.Add(action);
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
}
