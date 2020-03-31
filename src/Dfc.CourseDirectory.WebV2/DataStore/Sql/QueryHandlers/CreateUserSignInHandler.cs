using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.QueryHandlers
{
    public class CreateUserSignInHandler : ISqlQueryHandler<CreateUserSignIn, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, CreateUserSignIn query)
        {
            await MergeUser();
            await AddSignIn();

            return new None();

            async Task MergeUser()
            {
                var sql = @"
MERGE Pttcd.Users AS target
USING (
    SELECT @UserId UserId, @Email Email, @FirstName FirstName, @LastName LastName
) AS source
ON target.UserId = source.UserId
WHEN MATCHED THEN
    UPDATE SET
        target.Email = source.Email,
        target.FirstName = source.FirstName,
        target.LastName = source.LastName
WHEN NOT MATCHED THEN
    INSERT (UserId, Email, FirstName, LastName)
    VALUES (source.UserId, source.Email, source.FirstName, source.LastName);

IF @ProviderId IS NOT NULL
    MERGE Pttcd.UserProviders AS target
    USING (SELECT @UserId UserId, @ProviderId ProviderId) AS source
    ON target.UserId = source.UserId AND target.ProviderId = source.ProviderId
    WHEN NOT MATCHED THEN INSERT (UserId, ProviderId) VALUES (source.UserId, source.ProviderId);";

                var paramz = new
                {
                    query.User.UserId,
                    query.User.Email,
                    query.User.FirstName,
                    query.User.LastName,
                    ProviderId = query.CurrentProviderId
                };

                await transaction.Connection.ExecuteAsync(sql, paramz, transaction);
            }

            Task AddSignIn()
            {
                var sql = @"
INSERT INTO Pttcd.UserSignIns (UserId, SignedInUtc) VALUES (@UserId, @SignedInUtc)";

                var paramz = new { query.User.UserId, query.SignedInUtc };

                return transaction.Connection.ExecuteAsync(sql, paramz, transaction);
            }
        }
    }
}
