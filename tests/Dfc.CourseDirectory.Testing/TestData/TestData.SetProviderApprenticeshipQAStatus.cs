using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using System;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public Task SetProviderApprenticeshipQAStatus(Guid providerId, ApprenticeshipQAStatus status) =>
            WithSqlQueryDispatcher(async dispatcher =>
            {
                await dispatcher.ExecuteQuery(new SetProviderApprenticeshipQAStatus()
                {
                    ProviderId = providerId,
                    ApprenticeshipQAStatus = status
                });
            });
    }
}
