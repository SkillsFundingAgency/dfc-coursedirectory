using System;
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

    public class SqlQuerySpyDecorator : ISqlQueryDispatcher
    {
        private readonly ISqlQueryDispatcher _inner;
        private readonly SqlQuerySpy _spy;

        public SqlQuerySpyDecorator(ISqlQueryDispatcher inner, SqlQuerySpy sqlQuerySpy)
        {
            _inner = inner;
            _spy = sqlQuerySpy;
        }

        public SqlTransaction Transaction => _inner.Transaction;

        public Task<T> ExecuteQuery<T>(ISqlQuery<T> query)
        {
            _spy.RegisterCall(query);
            return _inner.ExecuteQuery(query);
        }
    }
}
