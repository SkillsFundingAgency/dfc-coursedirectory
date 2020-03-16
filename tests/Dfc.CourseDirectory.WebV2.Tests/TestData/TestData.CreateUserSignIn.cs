using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.WebV2.Tests
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
