using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Core.BackgroundWorkers
{
    public class QueueBackgroundWorkScheduler : BackgroundService, IBackgroundWorkScheduler
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger _logger;
        private readonly ConcurrentQueue<(WorkItem WorkItem, object State)> _work;
        private readonly SemaphoreSlim _gotWorkSignal;

        public QueueBackgroundWorkScheduler(
            IServiceScopeFactory serviceScopeFactory,
            ILoggerFactory loggerFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = loggerFactory.CreateLogger<QueueBackgroundWorkScheduler>();
            _work = new ConcurrentQueue<(WorkItem WorkItem, object State)>();
            _gotWorkSignal = new SemaphoreSlim(0);
        }

        public override void Dispose()
        {
            _gotWorkSignal.Dispose();
            _work.Clear();
        }

        public Task Schedule(WorkItem workItem, object state = null)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _work.Enqueue((workItem, state));
            _gotWorkSignal.Release();

            return Task.CompletedTask;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            _logger.LogInformation("WorkItemExecutorHostedService started");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);

            _logger.LogInformation("WorkItemExecutorHostedService stopped");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _gotWorkSignal.WaitAsync(stoppingToken);
                _work.TryDequeue(out var entry);
                Debug.Assert(entry != default);

                using var scope = _serviceScopeFactory.CreateScope();
                var scopeServices = scope.ServiceProvider;

                try
                {
                    await entry.WorkItem(entry.State, scopeServices, stoppingToken);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing work item.");
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }
        }
    }
}
