using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.Sql;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly SqlDataSync _sqlDataSync;
        private readonly IClock _clock;
        private readonly UniqueIdHelper _uniqueIdHelper;
        private readonly SemaphoreSlim _dispatcherLock = new SemaphoreSlim(1, 1);

        public TestData(
            ISqlQueryDispatcherFactory sqlQueryDispatcherFactory,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            SqlDataSync sqlDataSync,
            IClock clock,
            UniqueIdHelper uniqueIdHelper)
        {
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _sqlDataSync = sqlDataSync;
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
            await _dispatcherLock.WaitAsync();

            try
            {
                using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();
                var result = await action(dispatcher);
                await dispatcher.Commit();
                return result;
            }
            finally
            {
                _dispatcherLock.Release();
            }
        }
    }
}
