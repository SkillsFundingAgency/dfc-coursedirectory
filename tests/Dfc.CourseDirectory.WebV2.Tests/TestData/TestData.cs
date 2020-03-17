using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public partial class TestData
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IServiceProvider _serviceProvider;
        private readonly IClock _clock;

        public TestData(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            IServiceProvider serviceProvider,
            IClock clock)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
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
            var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var transaction = scope.ServiceProvider.GetRequiredService<SqlTransaction>();
                var queryDispatcher = scope.ServiceProvider.GetRequiredService<ISqlQueryDispatcher>();

                var result = await action(queryDispatcher);

                transaction.Commit();

                return result;
            }
        }
    }
}
