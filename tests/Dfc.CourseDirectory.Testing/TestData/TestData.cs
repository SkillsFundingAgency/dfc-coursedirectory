using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;
        private readonly IClock _clock;
        private readonly UniqueIdHelper _uniqueIdHelper;
        private readonly SemaphoreSlim _dispatcherLock = new SemaphoreSlim(1, 1);
        private AsyncLocal<ISqlQueryDispatcher> _currentDispatcher = new AsyncLocal<ISqlQueryDispatcher>();

        public TestData(
            ISqlQueryDispatcherFactory sqlQueryDispatcherFactory,
            IClock clock,
            UniqueIdHelper uniqueIdHelper)
        {
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
            _clock = clock;
            _uniqueIdHelper = uniqueIdHelper;
        }

        protected Task WithSqlQueryDispatcher(Func<ISqlQueryDispatcher, Task> action) =>
            WithSqlQueryDispatcher(async dispatcher =>
            {
                await action(dispatcher);
                return 0;
            });

        protected async Task<TResult> WithSqlQueryDispatcher<TResult>(
            Func<ISqlQueryDispatcher, Task<TResult>> action)
        {
            if (_currentDispatcher.Value != null)
            {
                return await action(_currentDispatcher.Value);
            }

            await _dispatcherLock.WaitAsync();

            try
            {
                using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();
                _currentDispatcher.Value = dispatcher;
                var result = await action(dispatcher);
                await dispatcher.Commit();
                return result;
            }
            finally
            {
                _dispatcherLock.Release();
                _currentDispatcher.Value = null;
            }
        }
    }
}
