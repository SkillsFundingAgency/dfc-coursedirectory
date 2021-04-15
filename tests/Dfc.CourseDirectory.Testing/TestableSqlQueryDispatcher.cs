using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;

namespace Dfc.CourseDirectory.Testing
{
    /// <summary>
    /// An implementation of <see cref="ISqlQueryDispatcher"/> for testing purposes.
    /// </summary>
    /// <remarks>
    /// Callers specify the result of queries by calling <see cref="SpecifyResult{TQuery, TResult}(TResult)"/>
    /// or <see cref="QueueResult{TQuery, TResult}(TResult)"/>.
    /// </remarks>
    public sealed class TestableSqlQueryDispatcher : ISqlQueryDispatcher
    {
        private bool _disposed;
        private readonly Dictionary<Type, (object Result, Action OnEmitted)> _queryResults;
        private readonly Dictionary<Type, Queue<(object Result, Action OnEmitted)>> _queuedQueryResults;
        private readonly object _gate = new object();

        public TestableSqlQueryDispatcher()
        {
            _queryResults = new Dictionary<Type, (object Result, Action OnEmitted)>();
            _queuedQueryResults = new Dictionary<Type, Queue<(object Result, Action OnEmitted)>>();
        }

        public SqlTransaction Transaction => throw new NotSupportedException();

        public Task Commit() => Task.CompletedTask;

        public void Dispose()
        {
            _disposed = true;
        }

        public Task<T> ExecuteQuery<T>(ISqlQuery<T> query)
        {
            ThrowIfDisposed();

            var queryType = query.GetType();

            lock (_gate)
            {
                if (_queuedQueryResults.TryGetValue(queryType, out var queue))
                {
                    if (queue.TryDequeue(out var queuedResult))
                    {
                        return ConsumeResult(queuedResult);
                    }
                }

                if (_queryResults.TryGetValue(queryType, out var fixedResult))
                {
                    return ConsumeResult(fixedResult);
                }

                static Task<T> ConsumeResult((object Result, Action OnEmitted) result)
                {
                    result.OnEmitted?.Invoke();
                    return Task.FromResult((T)result.Result);
                }
            }

            throw new Exception($"No results have been specified for {queryType.Name}.");
        }

        public IAsyncEnumerable<T> ExecuteQuery<T>(ISqlQuery<IAsyncEnumerable<T>> query)
        {
            throw new NotImplementedException();
        }

        public void SpecifyResult<TQuery, TResult>(TResult result, Action onResultEmitted = null)
        {
            var queryType = typeof(TQuery);

            lock (_gate)
            {
                _queryResults[queryType] = (result, onResultEmitted);
            }
        }

        public void QueueResult<TQuery, TResult>(TResult result, Action onResultEmitted = null)
        {
            var queryType = typeof(TQuery);

            lock (_gate)
            {
                if (!_queuedQueryResults.ContainsKey(queryType))
                {
                    _queuedQueryResults.Add(queryType, new Queue<(object Result, Action OnEmitted)>());
                }

                _queuedQueryResults[queryType].Enqueue((result, onResultEmitted));
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TestableSqlQueryDispatcher));
            }
        }
    }
}
