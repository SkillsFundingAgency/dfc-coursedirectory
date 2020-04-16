﻿using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Respawn;
using Xunit.Abstractions;
using Xunit.Sdk;
using CosmosDbQueryDispatcher = Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.CosmosDbQueryDispatcher;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class DatabaseFixture
    {
        private readonly Checkpoint _sqlCheckpoint;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _services;
        private readonly IMessageSink _messageSink;

        public DatabaseFixture(IConfiguration configuration, IServiceProvider services, IMessageSink messageSink)
        {
            _configuration = configuration;
            _services = services;
            _messageSink = messageSink;

            DeploySqlDb();
            _sqlCheckpoint = CreateCheckpoint();
        }

        public MutableClock Clock => _services.GetRequiredService<IClock>() as MutableClock;

        public Mock<CosmosDbQueryDispatcher> CosmosDbQueryDispatcher =>
            Mock.Get((CosmosDbQueryDispatcher)_services.GetRequiredService<ICosmosDbQueryDispatcher>());

        public InMemoryDocumentStore InMemoryDocumentStore => _services.GetRequiredService<InMemoryDocumentStore>();

        public SqlQuerySpy SqlQuerySpy => _services.GetRequiredService<SqlQuerySpy>();

        public TestData TestData => _services.GetRequiredService<TestData>();

        public TestUserInfo User => _services.GetRequiredService<TestUserInfo>();

        private string ConnectionString => _configuration["ConnectionStrings:DefaultConnection"];

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddCosmosDbDataStore();
            services.AddSingleton<IClock, MutableClock>();
            services.AddSingleton<InMemoryDocumentStore>();
            services.AddTransient<TestData>();
            services.AddSingleton<SqlQuerySpy>();
            services.Decorate<ISqlQueryDispatcher, SqlQuerySpyDecorator>();
            services.AddSingleton<TestUserInfo>();
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

            // Reset to the default calling user
            await User.Reset();
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
        
        private void DeploySqlDb()
        {
            var helper = new SqlDeployHelper();
            helper.Deploy(
                ConnectionString,
                writeMessage: message => _messageSink?.OnMessage(new DiagnosticMessage(message)));
        }
    }
}
