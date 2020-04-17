using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public Task CreateUserSignIn(string userId, DateTime signedInUtc) =>
            WithSqlQueryDispatcher(async dispatcher =>
            {
                var user = await dispatcher.ExecuteQuery(new GetUserById()
                {
                    UserId = userId
                });

                await dispatcher.ExecuteQuery(
                    new CreateUserSignIn()
                    {
                        User = user.AsT1,
                        SignedInUtc = signedInUtc
                    });
            });
    }
}
