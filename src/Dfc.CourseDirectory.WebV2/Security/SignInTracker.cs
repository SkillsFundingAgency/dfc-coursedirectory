using System.Security.Claims;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.WebV2.Security
{
    public class SignInTracker
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IClock _clock;

        public SignInTracker(
            ISqlQueryDispatcher sqlQueryDispatcher,
            IClock clock)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _clock = clock;
        }

        public Task RecordSignIn(ClaimsPrincipal principal)
        {
            var currentUser = ClaimsPrincipalCurrentUserProvider.MapUserInfoFromPrincipal(principal);

            return _sqlQueryDispatcher.ExecuteQuery(new CreateUserSignIn()
            {
                User = currentUser,
                SignedInUtc = _clock.UtcNow
            });
        }
    }
}
