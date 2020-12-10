using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public Task SetProviderTLevelDefinitions(Guid providerId, IEnumerable<Guid> tLevelDefinitionIds)
        {
            return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new SetProviderTLevelDefinitions
            {
                ProviderId = providerId,
                TLevelDefinitionIds = tLevelDefinitionIds
            }));
        }
    }
}
