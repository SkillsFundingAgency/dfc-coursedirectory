using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.BackgroundWorkers;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Testing
{
    public class ExecuteImmediatelyBackgroundWorkScheduler : IBackgroundWorkScheduler
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ExecuteImmediatelyBackgroundWorkScheduler(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Schedule(WorkItem workItem, object state = null)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            await workItem(state, scope.ServiceProvider, cancellationToken: default);
        }
    }
}
