using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class SetProviderApprenticeshipQAStatusHandler : ISqlQueryHandler<SetProviderApprenticeshipQAStatus, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, SetProviderApprenticeshipQAStatus query)
        {
            var sql = @"
MERGE INTO Pttcd.Providers AS target
USING (SELECT @ProviderId ProviderId, @ApprenticeshipQAStatus ApprenticeshipQAStatus) AS source
ON source.ProviderId = target.ProviderId
WHEN MATCHED THEN UPDATE SET ApprenticeshipQAStatus = source.ApprenticeshipQAStatus
WHEN NOT MATCHED THEN INSERT (ProviderId, ApprenticeshipQAStatus) VALUES (source.ProviderId, source.ApprenticeshipQAStatus);";

            var paramz = new
            {
                query.ProviderId,
                query.ApprenticeshipQAStatus
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new None();
        }
    }
}
