using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class CreateTLevelDefinitionHandler : ISqlQueryHandler<CreateTLevelDefinition, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, CreateTLevelDefinition query)
        {
            const string sql = @"
INSERT INTO Pttcd.TLevelDefinitions (
    TLevelDefinitionId,
    FrameworkCode,
    ProgType,
    Name
) VALUES (
    @TLevelDefinitionId,
    @FrameworkCode,
    @ProgType,
    @Name
)";
            await transaction.Connection.ExecuteAsync(sql, query, transaction);

            return new Success();
        }
    }
}
