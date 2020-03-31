using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2.Security
{
    public class EnsureApprenticeshipQAStatusSetSignInAction : ISignInAction
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public EnsureApprenticeshipQAStatusSetSignInAction(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task OnUserSignedIn(SignInContext context)
        {
            if (context.Provider?.ProviderType.HasFlag(ProviderType.Apprenticeships) ?? false)
            {
                await _sqlQueryDispatcher.ExecuteQuery(new EnsureApprenticeshipQAStatusSetForProvider()
                {
                    ProviderId = context.Provider.Id
                });
            }
        }
    }
}
