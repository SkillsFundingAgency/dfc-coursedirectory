using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.WebV2.Security
{
    public class SignInTracker : ISignInAction
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

        public Task RecordSignIn(AuthenticatedUserInfo userInfo) =>
            _sqlQueryDispatcher.ExecuteQuery(new CreateUserSignIn()
            {
                User = userInfo,
                SignedInUtc = _clock.UtcNow
            });

        Task ISignInAction.OnUserSignedIn(SignInContext context) => RecordSignIn(context.UserInfo);
    }
}
