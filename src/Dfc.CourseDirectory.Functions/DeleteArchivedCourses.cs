using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SqlQueries = Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Functions
{
    public class DeleteArchivedCourses
    {
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;
        private readonly IClock _clock;
        private readonly ILogger<DeleteArchivedCourses> _logger;

        public DeleteArchivedCourses(ISqlQueryDispatcherFactory sqlQueryDispatcherFactory, IClock clock, ILogger<DeleteArchivedCourses> logger)
        {
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
            _clock = clock;
            _logger = logger;
        }

        [Function("DeleteArchivedCourses")]
        public async Task Run([TimerTrigger("0 0 3 * * *")] TimerInfo timer)
        {
            _logger.LogInformation("C# Timer trigger function executed at: {DateTime}", DateTime.Now);

            if (timer.IsPastDue)
            {
                return;
            }

            using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();

            var fourWeeksOldDate = _clock.UtcNow.AddDays(-28);
            var fifteenMonthsOldDate = _clock.UtcNow.AddMonths(-15);
            var thirtyDaysOldDate = _clock.UtcNow.AddDays(-30);


            _logger.LogInformation("Calling the stored procedure to archive courses for providers with a type of 'None' that have remained unchanged for the past month as of {DateString}", fourWeeksOldDate.ToShortDateString());
            await dispatcher.ExecuteQuery(new SqlQueries.ArchiveProviderCourses() { RetentionDate = fourWeeksOldDate });

            _logger.LogInformation("Calling stored procedure to archive courses with a start date older than 15 months as of {DateString}", fifteenMonthsOldDate.ToShortDateString());
            await dispatcher.ExecuteQuery(new SqlQueries.ArchiveOldCourses() { RetentionDate = fifteenMonthsOldDate });

            _logger.LogInformation("Calling the stored procedure to remove records that have been archived for more than 30 days.");
            await dispatcher.ExecuteQuery(new SqlQueries.DeleteArchivedCourses() { RetentionDate = thirtyDaysOldDate });

            await dispatcher.Commit();
        }
    }
}
