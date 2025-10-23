using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Query = Dfc.CourseDirectory.Core.DataStore.Sql.Queries.CreateUser;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<UserInfo> CreateUser(
            string firstName = null,
            string lastName = null,
            Guid? providerId = default)
        {
            var userId = _uniqueIdHelper.GenerateUserId();
            var email = _uniqueIdHelper.GenerateUserEmail();
            firstName ??= Faker.Name.First();
            lastName ??= Faker.Name.Last();

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
