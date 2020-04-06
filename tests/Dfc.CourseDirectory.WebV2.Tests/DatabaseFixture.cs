using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Respawn;
using CosmosDbQueryDispatcher = Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.CosmosDbQueryDispatcher;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class DatabaseFixture
    {
        private readonly Checkpoint _sqlCheckpoint;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _services;

        public DatabaseFixture(IConfiguration configuration, IServiceProvider services)
        {
            _sqlCheckpoint = CreateCheckpoint();
            _configuration = configuration;
            _services = services;
        }

        public MutableClock Clock => _services.GetRequiredService<IClock>() as MutableClock;

        public Mock<CosmosDbQueryDispatcher> CosmosDbQueryDispatcher =>
            Mock.Get((CosmosDbQueryDispatcher)_services.GetRequiredService<ICosmosDbQueryDispatcher>());

        public InMemoryDocumentStore InMemoryDocumentStore => _services.GetRequiredService<InMemoryDocumentStore>();

        public SqlQuerySpy SqlQuerySpy { get; } = new SqlQuerySpy();

        public TestData TestData => _services.GetRequiredService<TestData>();

        private string ConnectionString => _configuration["ConnectionStrings:DefaultConnection"];

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddCosmosDbDataStore();
            services.AddSingleton<IClock, MutableClock>();
            services.AddSingleton<InMemoryDocumentStore>();
            services.AddTransient<TestData>();
            services.AddSingleton<SqlQuerySpy>();
            services.Decorate<ISqlQueryDispatcher, SqlQuerySpyDecorator>();
        }

        public void OnTestStarting()
        {
            // Reset calls on CosmosDbQueryDispatcher
            CosmosDbQueryDispatcher.Reset();

            // Clear in-memory DB
            InMemoryDocumentStore.Clear();

            // Clear spy calls
            SqlQuerySpy.Reset();
        }

        public async Task OnTestStartingAsync()
        {
            // Clear out all data from SQL database
            await _sqlCheckpoint.Reset(ConnectionString);
        }

        public Task WithSqlQueryDispatcher(Func<ISqlQueryDispatcher, Task> action) =>
            WithSqlQueryDispatcher(async dispatcher =>
            {
                await action(dispatcher);
                return 0;
            });

        public async Task<TResult> WithSqlQueryDispatcher<TResult>(
            Func<ISqlQueryDispatcher, Task<TResult>> action)
        {
            var serviceScopeFactory = _services.GetRequiredService<IServiceScopeFactory>();
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var transaction = scope.ServiceProvider.GetRequiredService<SqlTransaction>();
                var queryDispatcher = scope.ServiceProvider.GetRequiredService<ISqlQueryDispatcher>();

                var result = await action(queryDispatcher);

                transaction.Commit();

                return result;
            }
        }

        private Checkpoint CreateCheckpoint() => new Checkpoint()
        {
            SchemasToInclude = new[] { "Pttcd" }
        };
    }
}
