using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Models;
using Query = Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries.CreateUser;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public partial class TestData
    {
        public async Task<UserInfo> CreateUser(
            string userId,
            string email,
            string firstName,
            string lastName,
            Guid? providerId)
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
