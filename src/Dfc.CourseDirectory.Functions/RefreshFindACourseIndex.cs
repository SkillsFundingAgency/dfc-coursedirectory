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

            int updatedCourseRuns;
            int deletedCourseRuns;
            int deletedCourses;
            int total = 0;

            do
            {
                using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();

                //updated records:

                updatedCourseRuns = await dispatcher.ExecuteQuery(new UpdateFindACourseIndexFromMissingCourses()
                {
                    MaxCourseRunCount = batchSize,
                    CreatedBefore = createdBefore,
                    CreatedAfter = createdAfter,
                    Now = _clock.UtcNow
                });

                total += updatedCourseRuns;

                //audits the index and purges index records where the courserun no longer exists:

                deletedCourseRuns = await dispatcher.ExecuteQuery(new AuditAndSyncCourseRunsToIndex()
                {
                    MaxCourseRunCount = batchSize,
                    Now = _clock.UtcNow
                });

                total += deletedCourseRuns;

                //audits the index and purges index records where the course no longer exists:

                deletedCourses = await dispatcher.ExecuteQuery(new AuditAndSyncCoursesToIndex()
                {
                    MaxCourseCount = batchSize,
                    Now = _clock.UtcNow
                });

                total += deletedCourses;

                await dispatcher.Commit();
            }
            while (
                (updatedCourseRuns + deletedCourseRuns + deletedCourses) == batchSize &&
                (total + batchSize) <= maxRecordsPerInvocation &&
                !cancellationToken.IsCancellationRequested);
        }
    }
}
