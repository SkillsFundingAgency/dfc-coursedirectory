using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpdateFindACourseIndexForCourseRunsHandler : ISqlQueryHandler<UpdateFindACourseIndexForCourseRuns, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, UpdateFindACourseIndexForCourseRuns query)
        {
            var paramz = new
            {
                CourseRunIds = TvpHelper.CreateGuidIdTable(query.CourseRunIds),
                Now = query.Now
            };

            await transaction.Connection.ExecuteAsync(
                "Pttcd.RefreshFindACourseIndex",
                paramz,
                transaction,
                commandType: System.Data.CommandType.StoredProcedure);

            return new Success();
        }
    }
}
