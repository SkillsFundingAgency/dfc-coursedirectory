using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.BackgroundWorkers
{
    public delegate Task WorkItem(object state, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}
