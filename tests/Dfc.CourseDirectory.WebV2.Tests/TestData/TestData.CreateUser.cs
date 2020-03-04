using System.Threading.Tasks;
using Query = Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries.CreateUser;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public partial class TestData
    {
        public Task CreateUser(string userId, string email, string firstName, string lastName) =>
            WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new Query()
                {
                    UserId = userId,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName
                }));
    }
}
