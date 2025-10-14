using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.BackgroundWorkers
{
    public interface IBackgroundWorkScheduler
    {
        Task Schedule(WorkItem workItem, object state = null);
    }
}
