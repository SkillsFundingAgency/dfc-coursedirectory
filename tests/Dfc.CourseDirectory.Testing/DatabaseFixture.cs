using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Testing.DataStore.CosmosDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Respawn;
using Xunit.Abstractions;
using Xunit.Sdk;
using CosmosDbQueryDispatcher = Dfc.CourseDirectory.Testing.DataStore.CosmosDb.CosmosDbQueryDispatcher;

namespace Dfc.CourseDirectory.Testing
{
    public class DatabaseFixture : IDisposable
    {
        private const string LockName = "Tests";
        private const int LockTimeoutSeconds = 15 * 60;

        private readonly Checkpoint _sqlCheckpoint;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _services;
        private readonly IMessageSink _messageSink;
        private readonly SqlConnection _lockConnection;
        private readonly SqlTransaction _lockTransaction;

        public DatabaseFixture(IConfiguration configuration, IServiceProvider services, IMessageSink messageSink)
        {
            _configuration = configuration;
            _services = services;
            _messageSink = messageSink;

            _lockConnection = new SqlConnection(ConnectionString);
            _lockConnection.Open();
            _lockTransaction = _lockConnection.BeginTransaction();

            AcquireLock();

            DeploySqlDb();
            _sqlCheckpoint = CreateCheckpoint();
        }

        public MutableClock Clock => _services.GetRequiredService<IClock>() as MutableClock;

        public Mock<CosmosDbQueryDispatcher> CosmosDbQueryDispatcher =>
            Mock.Get((CosmosDbQueryDispatcher)_services.GetRequiredService<ICosmosDbQueryDispatcher>());

        public InMemoryDocumentStore InMemoryDocumentStore => _services.GetRequiredService<InMemoryDocumentStore>();

        public SqlQuerySpy SqlQuerySpy => _services.GetRequiredService<SqlQuerySpy>();

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
            services.AddTransient<SqlDataSync>();
        }

        public void Dispose()
        {
            _lockTransaction.Dispose();
            _lockConnection.Dispose();
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

        private void AcquireLock()
        {
            using var cmd = _lockConnection.CreateCommand();
            cmd.Transaction = _lockTransaction;
            cmd.CommandText = "sp_getapplock";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@Resource", LockName));
            cmd.Parameters.Add(new SqlParameter("@LockMode", "Exclusive"));
            cmd.CommandTimeout = LockTimeoutSeconds;

            var returnValueParameter = new SqlParameter("@RETURN_VALUE", SqlDbType.Int)
            {
                Direction = ParameterDirection.ReturnValue
            };
            cmd.Parameters.Add(returnValueParameter);

            cmd.ExecuteNonQuery();

            if ((int)returnValueParameter.Value < 0)
            {
                throw new Exception("Failed acquiring lock on test database.");
            }
        }

        private Checkpoint CreateCheckpoint() => new Checkpoint()
        {
            SchemasToInclude = new[] { "Pttcd" }
        };
        
        private void DeploySqlDb()
        {
            if ((Environment.GetEnvironmentVariable("CD_SkipTestSqlDeployment") ?? string.Empty)
                .Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                _messageSink.OnMessage(new DiagnosticMessage("Skipping database deployment"));
                return;
            }

            var helper = new SqlDeployHelper();
            helper.Deploy(
                ConnectionString,
                writeMessage: message => _messageSink?.OnMessage(new DiagnosticMessage(message)));
        }
    }
}
