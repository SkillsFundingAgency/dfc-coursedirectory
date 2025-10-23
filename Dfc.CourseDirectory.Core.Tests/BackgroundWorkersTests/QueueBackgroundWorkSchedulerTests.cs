using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.BackgroundWorkers;
using FluentAssertions;
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
            executedSignal.Wait(100).Should().BeTrue();
        }

        [Fact]
        public async Task DoesNotRethrowExceptionFromWorkItem()
        {
            // Arrange
            var serviceScopeFactory = CreateServiceScopeFactory();
            var loggerFactory = CreateLoggerFactory();
            using var scheduler = new QueueBackgroundWorkScheduler(serviceScopeFactory, loggerFactory);

            using var executedSignal = new ManualResetEventSlim(false);
            WorkItem workItem = (state, sp, ct) =>
            {
                try
                {
                    throw new Exception("Bang!");
                }
                finally
                {
                    executedSignal.Set();
                }
            };

            // Act
            await scheduler.StartAsync(cancellationToken: default);
            await scheduler.Schedule(workItem);

            // Assert
            executedSignal.Wait(100).Should().BeTrue();
        }

        [Fact]
        public async Task PassesStateToWorkItem()
        {
            // Arrange
            var serviceScopeFactory = CreateServiceScopeFactory();
            var loggerFactory = CreateLoggerFactory();
            using var scheduler = new QueueBackgroundWorkScheduler(serviceScopeFactory, loggerFactory);

            var workItemState = new object();

            object capturedState = null;

            using var executedSignal = new ManualResetEventSlim(false);
            WorkItem workItem = (state, sp, ct) =>
            {
                capturedState = state;
                executedSignal.Set();

                return Task.CompletedTask;
            };

            // Act
            await scheduler.StartAsync(cancellationToken: default);
            await scheduler.Schedule(workItem, workItemState);

            // Assert
            executedSignal.Wait(100).Should().BeTrue();
            capturedState.Should().Be(workItemState);
        }

        [Fact]
        public async Task PassesCancellationTokenToWorkItemThatIsCancelledWhenServiceStopped()
        {
            // Arrange
            var serviceScopeFactory = CreateServiceScopeFactory();
            var loggerFactory = CreateLoggerFactory();
            using var scheduler = new QueueBackgroundWorkScheduler(serviceScopeFactory, loggerFactory);

            var capturedCancellationToken = default(CancellationToken);

            using var executingSignal = new ManualResetEventSlim(false);

            WorkItem workItem = (state, sp, ct) =>
            {
                capturedCancellationToken = ct;
                executingSignal.Set();

                return Task.CompletedTask;
            };

            // Act
            await scheduler.StartAsync(cancellationToken: default);
            await scheduler.Schedule(workItem);

            // Assert
            executingSignal.Wait(100).Should().BeTrue();
            capturedCancellationToken.Should().NotBe(default);
            capturedCancellationToken.IsCancellationRequested.Should().BeFalse();

            // Act
            await scheduler.StopAsync(cancellationToken: default);

            // Assert
            capturedCancellationToken.IsCancellationRequested.Should().BeTrue();
        }

        [Fact]
        public async Task EachWorkItemHasOwnServiceScope()
        {
            // Arrange
            var serviceScopeFactory = CreateServiceScopeFactory();
            var loggerFactory = CreateLoggerFactory();
            using var scheduler = new QueueBackgroundWorkScheduler(serviceScopeFactory, loggerFactory);

            using var workItem1ExecutedSignal = new ManualResetEventSlim(false);
            SomeDependency capturedDependency1 = default;
            WorkItem workItem1 = (state, sp, ct) =>
            {
                capturedDependency1 = sp.GetRequiredService<SomeDependency>();
                workItem1ExecutedSignal.Set();
                return Task.CompletedTask;
            };

            using var workItem2ExecutedSignal = new ManualResetEventSlim(false);
            SomeDependency capturedDependency2 = default;
            WorkItem workItem2 = (state, sp, ct) =>
            {
                capturedDependency2 = sp.GetRequiredService<SomeDependency>();
                workItem2ExecutedSignal.Set();
                return Task.CompletedTask;
            };

            // Act
            await scheduler.StartAsync(cancellationToken: default);
            await scheduler.Schedule(workItem1);
            await scheduler.Schedule(workItem2);

            // Assert
            workItem1ExecutedSignal.Wait(100).Should().BeTrue();
            workItem2ExecutedSignal.Wait(100).Should().BeTrue();
            capturedDependency1.Should().NotBe(capturedDependency2);
        }

        [Fact]
        public async Task ScheduleAndWait_WithTimeoutNotExceeded_DequeuesItemExecutesAndReturnsTrue()
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
            var completed = await scheduler.ScheduleAndWait(workItem, TimeSpan.FromMilliseconds(100));

            // Assert
            completed.Should().BeTrue();
            executedSignal.Wait(100).Should().BeTrue();
        }

        [Fact]
        public async Task ScheduleAndWait_WithTimeoutExceeded_DequeuesItemExecutesAndReturnsFalse()
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
            var completed = await scheduler.ScheduleAndWait(workItem, TimeSpan.Zero);

            // Assert
            completed.Should().BeFalse();
            executedSignal.Wait(100).Should().BeTrue();
        }

        [Fact]
        public async Task ScheduleAndWait_WithTimeoutNotExceededAndExceptionThrown_DequeuesItemExecutesAndThrowsException()
        {
            // Arrange
            var serviceScopeFactory = CreateServiceScopeFactory();
            var loggerFactory = CreateLoggerFactory();
            using var scheduler = new QueueBackgroundWorkScheduler(serviceScopeFactory, loggerFactory);

            using var executedSignal = new ManualResetEventSlim(false);
            WorkItem workItem = (state, sp, ct) =>
            {
                try
                {
                    throw new Exception("Bang!");
                }
                finally
                {
                    executedSignal.Set();
                }
            };

            // Act
            await scheduler.StartAsync(cancellationToken: default);
            Func<Task> action = () => scheduler.ScheduleAndWait(workItem, TimeSpan.FromMilliseconds(100));

            // Assert
            await action.Should().ThrowExactlyAsync<Exception>().WithMessage("Bang!");
            executedSignal.Wait(100).Should().BeTrue();
        }

        [Fact]
        public async Task ScheduleAndWait_WithTimeoutExceededAndExceptionThrown_DequeuesItemExecutesAndReturnsFalse()
        {
            // Arrange
            var serviceScopeFactory = CreateServiceScopeFactory();
            var loggerFactory = CreateLoggerFactory();
            using var scheduler = new QueueBackgroundWorkScheduler(serviceScopeFactory, loggerFactory);

            using var executedSignal = new ManualResetEventSlim(false);
            WorkItem workItem = (state, sp, ct) =>
            {
                try
                {
                    throw new Exception("Bang!");
                }
                finally
                {
                    executedSignal.Set();
                }
            };

            // Act
            await scheduler.StartAsync(cancellationToken: default);
            var completed = await scheduler.ScheduleAndWait(workItem, TimeSpan.Zero);

            // Assert
            completed.Should().BeFalse();
            executedSignal.Wait(100).Should().BeTrue();
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
