using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public Task SetProviderNonLarsSubType(Guid providerId, IEnumerable<Guid> nonLarsSubTypeIds)
        {
            return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new SetProviderNonLarsSubTypes
            {
                ProviderId = providerId,
                NonLarsSubTypeIds = nonLarsSubTypeIds
            }));
        }
    }
}
