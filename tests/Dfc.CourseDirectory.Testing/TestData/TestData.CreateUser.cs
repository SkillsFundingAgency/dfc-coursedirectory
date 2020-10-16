using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Query = Dfc.CourseDirectory.Core.DataStore.Sql.Queries.CreateUser;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<UserInfo> CreateUser(
            string userId = "bobby-tables",
            string email = "bobby.tables@example.org",
            string firstName = "Bobby",
            string lastName = "Tables",
            Guid? providerId = default)
        {
            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new Query()
                {
                    UserId = userId,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    ProviderId = providerId
                }));

            return new UserInfo()
            {
                UserId = userId,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };
        }
    }
}
