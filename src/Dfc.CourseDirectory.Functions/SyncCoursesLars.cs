using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class SyncCoursesLars
    {
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;

        public SyncCoursesLars(ISqlQueryDispatcherFactory sqlQueryDispatcherFactory)
        {
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
        }

        [FunctionName(nameof(SyncCoursesLars))]
        [Singleton]
        public async Task RunAsync([TimerTrigger("0 0 6 * * *")] TimerInfo myTimer)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var sql = @"
Update Pttcd.Courses 
Set CourseStatus=0, UpdatedOn=GETDATE(), UpdatedBy='SyncCoursesLars'
where CourseStatus=1 and LearnAimRef not in (select LearnAimRef from LARS.LearningDelivery)

Update Pttcd.CourseRuns
Set CourseRunStatus=0, UpdatedOn=getDate(),UpdatedBy='SyncCoursesLars'
From Pttcd.CourseRuns cr
JOIN Pttcd.Courses c ON c.CourseId = cr.CourseId
WHERE cr.CourseRunStatus = 1 
AND c.LearnAimRef not in (select LearnAimRef from LARS.LearningDelivery)

Update Pttcd.FindACourseIndex
Set Live=0, UpdatedOn=getDate()
WHERE LearnAimRef not in (select LearnAimRef from LARS.LearningDelivery) and Live=1
";
                await dispatcher.Transaction.Connection.ExecuteAsync(sql, transaction: dispatcher.Transaction);

                await dispatcher.Commit();
            }
        }
    }
}
