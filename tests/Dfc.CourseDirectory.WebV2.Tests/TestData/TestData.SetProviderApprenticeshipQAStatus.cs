using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using System;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.Tests
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
