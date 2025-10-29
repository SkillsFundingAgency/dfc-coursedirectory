using System;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.BackgroundWorkers
{
    public static class BackgroundWorkSchedulerExtensions
    {
        public static async Task<bool> ScheduleAndWait(
            this IBackgroundWorkScheduler backgroundWorkScheduler,
            WorkItem workItem,
            TimeSpan timeout,
            object state = null)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            var tcs = new TaskCompletionSource<bool>();

            await backgroundWorkScheduler.Schedule(
                new WorkItem(async (state, serviceProvider, cancellationToken) =>
                {
                    try
                    {
                        await workItem(state, serviceProvider, cancellationToken);
                        tcs.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                }),
                state);

            if (await Task.WhenAny(Task.Delay(timeout), tcs.Task) != tcs.Task)
            {
                return false;
            }

            return await tcs.Task;
        }
    }
}
