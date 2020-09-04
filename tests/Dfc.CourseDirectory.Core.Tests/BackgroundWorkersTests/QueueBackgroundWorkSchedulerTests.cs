using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.BackgroundWorkers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.BackgroundWorkersTests
{
    public class QueueBackgroundWorkSchedulerTests
    {
        [Fact]
        public async Task DequeuesWorkItemAndExecutes()
        {
            // Arrange
            var serviceScopeFactory = CreateServiceScopeFactory();
            var loggerFactory = CreateLoggerFactory();
            using var scheduler = new QueueBackgroundWorkScheduler(serviceScopeFactory, loggerFactory);

            using var executedSignal = new ManualResetEventSlim(false);
            WorkItem workItem = (state, sp, ct) =>
            {
                executedSignal.Set();
                return Task.CompletedTask;
            };

            // Act
            await scheduler.StartAsync(cancellationToken: default);
            await scheduler.Schedule(workItem);

            // Assert
            executedSignal.Wait(100);
        }

        [Fact]
        public async Task DoesNotRethrowExceptionFromWorkItem()
        {
            // Arrange
            var serviceScopeFactory = CreateServiceScopeFactory();
            var loggerFactory = CreateLoggerFactory();
            using var scheduler = new QueueBackgroundWorkScheduler(serviceScopeFactory, loggerFactory);

            using var workItem1ExecutedSignal = new ManualResetEventSlim(false);
            WorkItem workItem1 = (state, sp, ct) =>
            {
                try
                {
                    throw new Exception("Bang!");
                }
                finally
                {
                    workItem1ExecutedSignal.Set();
                }
            };

            using var workItem2ExecutedSignal = new ManualResetEventSlim(false);
            WorkItem workItem2 = (state, sp, ct) =>
            {
                workItem2ExecutedSignal.Set();
                return Task.CompletedTask;
            };

            // Act
            await scheduler.StartAsync(cancellationToken: default);
            await scheduler.Schedule(workItem1);
            workItem1ExecutedSignal.Wait(100);
            workItem2ExecutedSignal.Wait(100);

            // Assert
            await scheduler.StopAsync(cancellationToken: default);
        }

        [Fact]
        public async Task PassesStateToWorkItem()
        {
            // Arrange
            var serviceScopeFactory = CreateServiceScopeFactory();
            var loggerFactory = CreateLoggerFactory();
            using var scheduler = new QueueBackgroundWorkScheduler(serviceScopeFactory, loggerFactory);

            var workItemState = new object();

            object itemState = null;

            using var executedSignal = new ManualResetEventSlim(false);
            WorkItem workItem = (state, sp, ct) =>
            {
                itemState = state;

                executedSignal.Set();
                return Task.CompletedTask;
            };

            // Act
            await scheduler.StartAsync(cancellationToken: default);
            await scheduler.Schedule(workItem, workItemState);
            executedSignal.Wait(100);

            // Assert 
            Assert.Same(workItemState, itemState);
        }

        [Fact]
        public async Task PassesCancellationTokenToWorkItemThatIsCancelledWhenServiceStopped()
        {
            // Arrange
            var serviceScopeFactory = CreateServiceScopeFactory();
            var loggerFactory = CreateLoggerFactory();
            using var scheduler = new QueueBackgroundWorkScheduler(serviceScopeFactory, loggerFactory);

            CancellationToken itemCancellationToken = default;

            using var executingSignal = new ManualResetEventSlim(false);

            WorkItem workItem = (state, sp, ct) =>
            {
                itemCancellationToken = ct;

                executingSignal.Set();
                return Task.CompletedTask;
            };

            // Act & Assert
            await scheduler.StartAsync(cancellationToken: default);
            await scheduler.Schedule(workItem);
            executingSignal.Wait(100);
            Assert.False(itemCancellationToken.IsCancellationRequested);
            await scheduler.StopAsync(cancellationToken: default);
            Assert.True(itemCancellationToken.IsCancellationRequested);
        }

        [Fact]
        public async Task EachWorkItemHasOwnServiceScope()
        {
            // Arrange
            var serviceScopeFactory = CreateServiceScopeFactory();
            var loggerFactory = CreateLoggerFactory();
            using var scheduler = new QueueBackgroundWorkScheduler(serviceScopeFactory, loggerFactory);

            using var workItem1ExecutedSignal = new ManualResetEventSlim(false);
            SomeDependency workItem1Dependency = default;
            WorkItem workItem1 = (state, sp, ct) =>
            {
                workItem1Dependency = sp.GetRequiredService<SomeDependency>();
                workItem1ExecutedSignal.Set();
                return Task.CompletedTask;
            };

            using var workItem2ExecutedSignal = new ManualResetEventSlim(false);
            SomeDependency workItem2Dependency = default;
            WorkItem workItem2 = (state, sp, ct) =>
            {
                workItem2Dependency = sp.GetRequiredService<SomeDependency>();
                workItem2ExecutedSignal.Set();
                return Task.CompletedTask;
            };

            // Act
            await scheduler.StartAsync(cancellationToken: default);
            await scheduler.Schedule(workItem1);
            await scheduler.Schedule(workItem2);
            workItem1ExecutedSignal.Wait(100);
            workItem2ExecutedSignal.Wait(100);

            // Assert
            Assert.NotSame(workItem1Dependency, workItem2Dependency);
        }

        private static IServiceScopeFactory CreateServiceScopeFactory() =>
            new ServiceCollection()
                .AddScoped<SomeDependency>()
                .BuildServiceProvider()
                .GetRequiredService<IServiceScopeFactory>();

        private static ILoggerFactory CreateLoggerFactory() => new LoggerFactory();

        private class SomeDependency { }
    }
}
