using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<Guid> CreateProviderNonLarsSubType(Guid providerId, Guid? nonLarsSubTypeId = null)
        {
            var id = nonLarsSubTypeId ?? Guid.NewGuid();
            var result = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new CreateProviderNonLarsSubType
            {
                NonLarsSubTypeId = id,
                ProviderId = providerId,
            }));

            return id;
        }

        public Task<IReadOnlyCollection<NonLarsSubType>> CreateInitialProviderNonLarsSubTypes(Guid providerId) =>
            WithSqlQueryDispatcher(async dispatcher =>
            {
                var createCommands = new[]
                {
                    new CreateProviderNonLarsSubType()
                    {
                        NonLarsSubTypeId = new Guid("236d0a46-adea-44c6-83d7-b5b2196e1bd3"),
                        ProviderId = providerId
                    },
                    new CreateProviderNonLarsSubType()
                    {
                        NonLarsSubTypeId = new Guid("024e3b2c-aa65-4808-8d17-74697c03591a"),
                        ProviderId = providerId
                    }
                    
                };

                foreach (var createCommand in createCommands)
                {
                    await dispatcher.ExecuteQuery(createCommand);
                }

                return await dispatcher.ExecuteQuery(new GetAllNonLarsSubTypes());
            });
    }
}
