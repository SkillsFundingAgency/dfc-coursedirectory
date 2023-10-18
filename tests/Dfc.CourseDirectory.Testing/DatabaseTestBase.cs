using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Dfc.CourseDirectory.Testing
{
    public class DatabaseTestBaseFixture : IDisposable
    {
        public DatabaseTestBaseFixture(IMessageSink messageSink)
        {
            var config = GetConfiguration();
            Services = CreateServiceProvider(config);
            DatabaseFixture = new DatabaseFixture(config, Services, messageSink);
        }

        public DatabaseFixture DatabaseFixture { get; }

        public IServiceProvider Services { get; }

        public IServiceScopeFactory ServiceScopeFactory => DatabaseFixture.ServiceScopeFactory;

        public TestData CreateTestData() => Services.GetRequiredService<TestData>();

        public void Dispose() => DatabaseFixture.Dispose();

        public void OnTestStarting()
        {
            DatabaseFixture.OnTestStarting();
        }

        public async Task OnTestStartingAsync()
        {
            await DatabaseFixture.OnTestStartingAsync();
        }

        private static IServiceProvider CreateServiceProvider(IConfiguration config)
        {
            var connectionString = config["ConnectionStrings:DefaultConnection"];

            var services = new ServiceCollection();

            services.AddSqlDataStore(connectionString);
            DatabaseFixture.ConfigureServices(services);
            return services.BuildServiceProvider();
        }

        private static IConfiguration GetConfiguration() => new ConfigurationBuilder()
            .AddTestConfigurationSources()
            .Build();
    }

    public abstract class DatabaseTestBase : IAsyncLifetime
    {
        protected DatabaseTestBase(DatabaseTestBaseFixture fixture)
        {
            Fixture = fixture;
            TestData = fixture.CreateTestData();
            Fixture.OnTestStarting();
        }

        protected MutableClock Clock => Fixture.DatabaseFixture.Clock;


        public DatabaseTestBaseFixture Fixture { get; }

        public IServiceProvider Services => Fixture.Services;

        public ISqlQueryDispatcherFactory SqlQueryDispatcherFactory => Fixture.DatabaseFixture.SqlQueryDispatcherFactory;

        public TestData TestData { get; }

        public Task DisposeAsync() => Task.CompletedTask;

        public Task InitializeAsync() => Fixture.OnTestStartingAsync();

        protected Task WithSqlQueryDispatcher(Func<ISqlQueryDispatcher, Task> action) =>
            Fixture.DatabaseFixture.WithSqlQueryDispatcher(action);

        protected Task<TResult> WithSqlQueryDispatcher<TResult>(
            Func<ISqlQueryDispatcher, Task<TResult>> action) =>
            Fixture.DatabaseFixture.WithSqlQueryDispatcher(action);
    }
}
