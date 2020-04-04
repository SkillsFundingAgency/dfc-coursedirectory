using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Dfc.CourseDirectory.WebV2
{
    public class RunStartupTasksHostedService : IHostedService
    {
        private readonly IEnumerable<IStartupTask> _startupTasks;

        public RunStartupTasksHostedService(IEnumerable<IStartupTask> startupTasks)
        {
            _startupTasks = startupTasks;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var t in _startupTasks)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                await t.Execute();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
