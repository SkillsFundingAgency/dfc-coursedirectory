using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Microsoft.Azure.Functions.Worker;

namespace Dfc.CourseDirectory.Functions
{
    public class NormalizeClassroomBasedCourseRunFields
    {
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;

        public NormalizeClassroomBasedCourseRunFields(ISqlQueryDispatcherFactory sqlQueryDispatcherFactory)
        {
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
        }

        [Function(nameof(NormalizeClassroomBasedCourseRunFields))]
        public async Task Execute([HttpTrigger(AuthorizationLevel.Function, "get", "post")] string input)
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
