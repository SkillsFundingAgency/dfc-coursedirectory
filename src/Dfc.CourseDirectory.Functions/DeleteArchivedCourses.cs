using System;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using SqlQueries = Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Functions
{
    public class DeleteArchivedCourses
    {
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;
        private readonly IClock _clock;

        public DeleteArchivedCourses(ISqlQueryDispatcherFactory sqlQueryDispatcherFactory, IClock clock)
        {
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
            _clock = clock;
        }

        [FunctionName("DeleteArchivedCourses")]
        [Singleton]
        public async Task Run([TimerTrigger("0 */10 * * * *")]TimerInfo timer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            if (timer.IsPastDue)
            {
                return;
            }

            using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();

            log.LogInformation($"Calling stored procedure to remove redundant records");
            await dispatcher.ExecuteQuery(new SqlQueries.DeleteArchivedCourses() { RetentionDate = DateTime.Now.AddDays(-30)});
        }
    }
}
