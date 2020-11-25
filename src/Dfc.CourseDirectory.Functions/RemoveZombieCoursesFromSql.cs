using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class RemoveZombieCoursesFromSql
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public RemoveZombieCoursesFromSql(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        [FunctionName(nameof(RemoveZombieCoursesFromSql))]
        [NoAutomaticTrigger]
        public Task Run(string input)
        {
            var sql = @"
select CourseId into #CourseIds from Pttcd.Courses where LastSyncedFromCosmos is null

delete Pttcd.CourseRunRegions
from Pttcd.CourseRunRegions crr
join Pttcd.CourseRuns cr on crr.CourseRunId = cr.CourseRunId
join #CourseIds x on x.CourseId = cr.CourseId

delete Pttcd.CourseRuns
from Pttcd.CourseRuns cr
join #CourseIds x on x.CourseId = cr.CourseId

delete Pttcd.Courses
from Pttcd.Courses c
join #CourseIds x on x.CourseId = c.CourseId

drop table #CourseIds";

            return _sqlQueryDispatcher.Transaction.Connection.ExecuteAsync(
                sql,
                transaction: _sqlQueryDispatcher.Transaction);
        }
    }
}
