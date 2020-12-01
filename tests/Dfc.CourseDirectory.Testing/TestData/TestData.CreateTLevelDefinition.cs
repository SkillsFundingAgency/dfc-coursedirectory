using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<Guid> CreateTLevelDefinition(Guid? tLevelDefinitionId = null, int frameworkCode = 123, int progType = 456, string name = "Test T Level")
        {
            var id = tLevelDefinitionId ?? Guid.NewGuid();

            var result = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new CreateTLevelDefinition
            {
                TLevelDefinitionId = id,
                FrameworkCode = frameworkCode,
                ProgType = progType,
                Name = name
            }));

            return id;
        }
    }
}
