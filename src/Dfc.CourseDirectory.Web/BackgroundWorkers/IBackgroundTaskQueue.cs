using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.BackgroundWorkers
{
    public interface IBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);

        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
}
