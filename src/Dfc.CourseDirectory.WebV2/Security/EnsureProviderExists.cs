using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Query = Dfc.CourseDirectory.Core.DataStore.Sql.Queries.EnsureProviderExists;

namespace Dfc.CourseDirectory.WebV2.Security
{
    public class EnsureProviderExists : ISignInAction
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public EnsureProviderExists(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task OnUserSignedIn(SignInContext context)
        {
            if (context.Provider != null)
            {
                await _sqlQueryDispatcher.ExecuteQuery(
                    new Query()
                    {
                        ProviderId = context.Provider.Id
                    });
            }
        }
    }
}
