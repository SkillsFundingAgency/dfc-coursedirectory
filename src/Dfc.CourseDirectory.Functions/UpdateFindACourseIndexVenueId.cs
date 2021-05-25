using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class UpdateFindACourseIndexVenueId
    {
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;

        public UpdateFindACourseIndexVenueId(ISqlQueryDispatcherFactory sqlQueryDispatcherFactory)
        {
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
        }

        [FunctionName(nameof(UpdateFindACourseIndexVenueId))]
        [NoAutomaticTrigger]
        public async Task Run(string input)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var sql = @"
UPDATE Pttcd.FindACourseIndex SET VenueId = cr.VenueId
FROM Pttcd.FindACourseIndex i
JOIN Pttcd.CourseRuns cr ON i.CourseRunId = cr.CourseRunId
WHERE cr.VenueId IS NOT NULL
AND i.Live = 1

UPDATE Pttcd.FindACourseIndex SET VenueId = tl.VenueId
FROM Pttcd.FindACourseIndex i
JOIN Pttcd.TLevelLocations tl ON i.TLevelLocationId = tl.TLevelLocationId
";

                await dispatcher.Transaction.Connection.ExecuteAsync(sql, transaction: dispatcher.Transaction);

                await dispatcher.Commit();
            }
        }
    }
}
