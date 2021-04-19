using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Moq;

namespace Dfc.CourseDirectory.Testing
{
    public class SqlQuerySpy
    {
        private readonly Mock<ISqlQueryDispatcher> _dispatcherMock;

        public SqlQuerySpy()
        {
            _dispatcherMock = new Mock<ISqlQueryDispatcher>();
        }

        public void Callback<TQuery, TResult>(Action<TQuery> action)
            where TQuery : ISqlQuery<TResult>
        {
            _dispatcherMock
                .Setup(d => d.ExecuteQuery(It.IsAny<TQuery>()))
                .Callback<ISqlQuery<TResult>>(query => action((TQuery)query));
        }

        internal void RegisterCall<T>(ISqlQuery<T> query)
        {
            _dispatcherMock.Object.ExecuteQuery(query);
        }

        public void Reset() => _dispatcherMock.Reset();

        public void VerifyQuery<TQuery, TResult>(Predicate<TQuery> match)
            where TQuery : ISqlQuery<TResult>
        {
            _dispatcherMock.Verify(d => d.ExecuteQuery(Match.Create(match)));
        }
    }

    public class SqlQuerySpyDispatcherFactoryDecorator : ISqlQueryDispatcherFactory
    {
        private readonly ISqlQueryDispatcherFactory _innerFactory;
        private readonly SqlQuerySpy _sqlQuerySpy;

        public SqlQuerySpyDispatcherFactoryDecorator(ISqlQueryDispatcherFactory innerFactory, SqlQuerySpy sqlQuerySpy)
        {
            _innerFactory = innerFactory;
            _sqlQuerySpy = sqlQuerySpy;
        }

        public ISqlQueryDispatcher CreateDispatcher(IsolationLevel isolationLevel = IsolationLevel.Snapshot)
        {
            return new Dispatcher(_innerFactory.CreateDispatcher(isolationLevel), _sqlQuerySpy);
        }

        private class Dispatcher : ISqlQueryDispatcher
        {
            private readonly ISqlQueryDispatcher _inner;
            private readonly SqlQuerySpy _spy;

            public Dispatcher(ISqlQueryDispatcher inner, SqlQuerySpy sqlQuerySpy)
            {
                _inner = inner;
                _spy = sqlQuerySpy;
            }

            public SqlTransaction Transaction => _inner.Transaction;

            public Task Commit() => _inner.Commit();

            public void Dispose() => _inner.Dispose();

            public Task<T> ExecuteQuery<T>(ISqlQuery<T> query)
            {
                _spy.RegisterCall(query);
                return _inner.ExecuteQuery(query);
            }

            public IAsyncEnumerable<T> ExecuteQuery<T>(ISqlQuery<IAsyncEnumerable<T>> query)
            {
                _spy.RegisterCall(query);
                return _inner.ExecuteQuery(query);
            }
        }
    }
}
