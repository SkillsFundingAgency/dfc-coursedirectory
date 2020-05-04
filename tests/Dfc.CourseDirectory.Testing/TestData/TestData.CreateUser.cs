using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Query = Dfc.CourseDirectory.Core.DataStore.Sql.Queries.CreateUser;

namespace Dfc.CourseDirectory.Testing
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
