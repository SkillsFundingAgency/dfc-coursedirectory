using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly SqlDataSync _sqlDataSync;
        private readonly IServiceProvider _serviceProvider;
        private readonly IClock _clock;
        private readonly SemaphoreSlim _dispatcherLock = new SemaphoreSlim(1, 1);

        public TestData(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            SqlDataSync sqlDataSync,
            IServiceProvider serviceProvider,
            IClock clock)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _sqlDataSync = sqlDataSync;
            _serviceProvider = serviceProvider;
            _clock = clock;
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
                var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var queryDispatcher = scope.ServiceProvider.GetRequiredService<ISqlQueryDispatcher>();

                    var result = await action(queryDispatcher);

                    queryDispatcher.Transaction.Commit();

                    return result;
                }
            }
            finally
            {
                _dispatcherLock.Release();
            }
        }
    }
}
