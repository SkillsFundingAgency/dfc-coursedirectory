using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Microsoft.Azure.Functions.Worker;

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

        [Function("RefreshFindACourseIndex")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo timer, CancellationToken cancellationToken)
        {

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
            int deleted;
            int total = 0;

            do
            {
                using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();

                //udpated records:

                updated = await dispatcher.ExecuteQuery(new UpdateFindACourseIndexFromMissingCourses()
                {
                    MaxCourseRunCount = batchSize,
                    CreatedBefore = createdBefore,
                    CreatedAfter = createdAfter,
                    Now = _clock.UtcNow
                });

                total += updated;

                //audits the index and purges index records where the course or courserun no longer exists:

                deleted = await dispatcher.ExecuteQuery(new AuditAndSyncFindACourseIndex()
                {
                    MaxCourseRunCount = batchSize,
                    Now = _clock.UtcNow
                });

                total += deleted;

                await dispatcher.Commit();
            }
            while (
                (updated + deleted) == batchSize &&
                (total + batchSize) <= maxRecordsPerInvocation &&
                !cancellationToken.IsCancellationRequested);
        }
    }
}
