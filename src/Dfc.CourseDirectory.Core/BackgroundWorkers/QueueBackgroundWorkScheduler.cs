using System;
using System.Threading;
using System.Threading.Channels;
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
        private readonly Channel<(WorkItem WorkItem, object State)> _work;

        public QueueBackgroundWorkScheduler(
            IServiceScopeFactory serviceScopeFactory,
            ILoggerFactory loggerFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = loggerFactory.CreateLogger<QueueBackgroundWorkScheduler>();
            _work = Channel.CreateUnbounded<(WorkItem WorkItem, object State)>();
        }

        public async Task Schedule(WorkItem workItem, object state = null)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            await _work.Writer.WriteAsync((workItem, state));
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
                await foreach (var entry in _work.Reader.ReadAllAsync(stoppingToken))
                {
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

        public override void Dispose()
        {
            _work.Writer.Complete();
            base.Dispose();
        }
    }
}
