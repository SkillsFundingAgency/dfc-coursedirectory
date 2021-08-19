using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class RefreshFindACourseIndex
    {
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;
        private readonly IClock _clock;

        public RefreshFindACourseIndex(ISqlQueryDispatcherFactory sqlQueryDispatcherFactory, IClock clock)
        {
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
            _clock = clock;
        }

        [FunctionName("RefreshFindACourseIndex")]
        [Singleton]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo timer, CancellationToken cancellationToken)
        {
            // This function exists to ensure any courses added via Data Management are added to the FAC API index.
            // (Data Management is currently the only place that doesn't synchronously update the index.)

            if (timer.IsPastDue)
            {
                return;
            }

            const int batchSize = 100;
            const int maxRecordsPerInvocation = 10000;

            // Add a filter to exclude courses that were created some time ago.
            // (We need this so we don't repeatedly try to re-index invalid courses e.g. where LARS data is missing.)
            var createdAfter = _clock.UtcNow.AddDays(-1);

            // Exclude course runs that have only just been created so we don't race with the background worker
            var createdBefore = _clock.UtcNow.AddHours(-1);

            int updated;
            int total = 0;

            do
            {
                using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();

                updated = await dispatcher.ExecuteQuery(new UpdateFindACourseIndexFromMissingCourses()
                {
                    MaxCourseRunCount = batchSize,
                    CreatedBefore = createdBefore,
                    CreatedAfter = createdAfter,
                    Now = _clock.UtcNow
                });

                total += updated;

                await dispatcher.Commit();
            }
            while (
                updated == batchSize &&
                (total + batchSize) <= maxRecordsPerInvocation &&  
                !cancellationToken.IsCancellationRequested);
        }
    }
}
