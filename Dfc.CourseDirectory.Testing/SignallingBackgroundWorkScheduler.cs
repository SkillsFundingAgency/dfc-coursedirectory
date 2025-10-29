using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.BackgroundWorkers;

namespace Dfc.CourseDirectory.Testing
{
    public class SignallingBackgroundWorkScheduler : IBackgroundWorkScheduler
    {
        private readonly IBackgroundWorkScheduler _innerScheduler;
        private readonly Action _onScheduled;

        public SignallingBackgroundWorkScheduler(
            IBackgroundWorkScheduler innerScheduler,
            Action onScheduled)
        {
            _innerScheduler = innerScheduler;
            _onScheduled = onScheduled;
        }

        public async Task Schedule(WorkItem workItem, object state = null)
        {
            try
            {
                await _innerScheduler.Schedule(workItem, state);
            }
            finally
            {
                _onScheduled();
            }
        }
    }
}
