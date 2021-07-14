using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class NormalizeClassroomBasedCourseRunFields
    {
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;

        public NormalizeClassroomBasedCourseRunFields(ISqlQueryDispatcherFactory sqlQueryDispatcherFactory)
        {
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
        }

        [FunctionName(nameof(NormalizeClassroomBasedCourseRunFields))]
        [NoAutomaticTrigger]
        public async Task Execute(string input)
        {
            using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();

            var sql = @"
UPDATE Pttcd.CourseRuns SET AttendancePattern = NULL WHERE AttendancePattern = 0
UPDATE Pttcd.CourseRuns SET StudyMode = NULL WHERE StudyMode = 0";

            await dispatcher.Transaction.Connection.ExecuteAsync(sql, transaction: dispatcher.Transaction);

            await dispatcher.Commit();
        }
    }
}
